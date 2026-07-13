export function initials(name: string, max = 2): string {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return '?'
  if (parts.length === 1) return parts[0].slice(0, max).toUpperCase()
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
}
