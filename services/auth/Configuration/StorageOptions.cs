namespace Auth.Api.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string ServiceUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool ForcePathStyle { get; set; }
    public string AvatarBucket { get; set; } = "user-avatars";
}
