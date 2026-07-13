using System.Security.Claims;
using Auth.Api.Configuration;
using Auth.Api.Data;
using Auth.Api.Dtos;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(
    AppDbContext db,
    IStorageService storageService,
    IOptions<StorageOptions> storageOptions) : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    private const long MaxAvatarBytes = 2 * 1024 * 1024; // 2 MB

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> List()
    {
        var list = await db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserDto(
                u.Id,
                u.Username,
                u.AvatarKey == null ? null : "/api/users/avatar/" + u.AvatarKey))
            .ToListAsync();

        return Ok(list);
    }

    [Authorize]
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Arquivo invalido." });

        if (file.Length > MaxAvatarBytes)
            return BadRequest(new { message = "Avatar deve ter no maximo 2 MB." });

        if (string.IsNullOrWhiteSpace(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Formato nao suportado. Use JPEG, PNG, WebP ou GIF." });

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token invalido." });

        var user = await db.Users.FindAsync([userId], cancellationToken);
        if (user is null)
            return NotFound(new { message = "Usuario nao encontrado." });

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = file.ContentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".jpg"
            };

        var bucket = storageOptions.Value.AvatarBucket;
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        await using var stream = file.OpenReadStream();
        var fileKey = await storageService.UploadFileAsync(
            bucket,
            fileName,
            stream,
            file.ContentType,
            cancellationToken);

        user.AvatarKey = fileKey;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { AvatarUrl = $"/api/users/avatar/{fileKey}" });
    }

    [HttpGet("avatar/{fileName}")]
    public async Task<IActionResult> GetAvatar(string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains(".."))
            return BadRequest(new { message = "Nome de arquivo invalido." });

        try
        {
            var s3Object = await storageService.GetFileAsync(
                storageOptions.Value.AvatarBucket,
                fileName,
                cancellationToken);

            return File(s3Object.Stream, s3Object.ContentType);
        }
        catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound(new { message = "Avatar nao encontrado." });
        }
    }
}
