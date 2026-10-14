namespace Acme.Workforce.Kit.Text;

/// <summary>Functions of text.</summary>
public static class Text
{
    /// <summary>Trims a value and lowers its case, so lookups ignore how people typed it.</summary>
    public static string Normalize(string s)
    {
        var trimmed = s.Trim();
        if (trimmed.Length == 0)
        {
            return "";
        }
        return trimmed.ToLowerInvariant();
    }

    /// <summary>Turns a display name into a lowercase, dash-separated key.</summary>
    public static string Slugify(string s)
    {
        var parts = s.Trim().ToLowerInvariant().Split(" ").ToList();
        var kept = new List<string>();
        foreach (var part in parts)
        {
            if (part.Length > 0)
            {
                kept.Add(part);
            }
        }
        return string.Join("-", kept);
    }

    /// <summary>Shortens a value to at most limit characters, marking the cut with an ellipsis.</summary>
    public static string Truncate(string s, int limit)
    {
        if (s.Length <= limit)
        {
            return s;
        }
        if (limit <= 3)
        {
            return s[..limit];
        }
        return s[..(limit - 3)] + "...";
    }

    /// <summary>Masks all but the last keep characters, as receipts and logs show references.</summary>
    public static string MaskTail(string s, int keep)
    {
        if (s.Length <= keep)
        {
            return s;
        }
        var hidden = s.Length - keep;
        return string.Concat(Enumerable.Repeat("*", hidden)) + s[^keep..];
    }

    public static int WordCount(string s)
    {
        var count = 0;
        foreach (var word in s.Trim().Split(" ").ToList())
        {
            if (word.Length > 0)
            {
                count += 1;
            }
        }
        return count;
    }

    /// <summary>Reports whether any of the words occurs in s, ignoring case.</summary>
    public static bool ContainsAny(string s, List<string> words)
    {
        var lowered = s.ToLowerInvariant();
        foreach (var word in words)
        {
            if (lowered.Contains(word.ToLowerInvariant(), StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    public static string JoinNonEmpty(List<string> parts, string sep)
    {
        var kept = new List<string>();
        foreach (var part in parts)
        {
            var clean = part.Trim();
            if (clean != "")
            {
                kept.Add(clean);
            }
        }
        return string.Join(sep, kept);
    }

    public static string FirstNonEmpty(List<string> parts)
    {
        foreach (var part in parts)
        {
            if (part.Trim() != "")
            {
                return part;
            }
        }
        return "";
    }

    /// <summary>Pads or cuts a value to exactly width characters, for fixed-width output.</summary>
    public static string Fit(string s, int width)
    {
        if (s.Length > width)
        {
            return s[..width];
        }
        return s.PadRight(width, ' ');
    }
}
