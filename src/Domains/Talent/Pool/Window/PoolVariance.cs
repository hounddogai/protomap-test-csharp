namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool variance is in its life cycle.</summary>
public enum PoolVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pool variance and its current status.</summary>
public sealed class PoolVariance(string code, string label, int revision, string locale, int updatedAt, List<string> tags, string channel, PoolVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string Locale { get; set; } = locale;
    public int UpdatedAt { get; set; } = updatedAt;
    public List<string> Tags { get; set; } = tags;
    public string Channel { get; set; } = channel;
    public PoolVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a pool variance in status created.</summary>
    public static PoolVariance NewPoolVariance(string code, string label)
    {
        return new PoolVariance(code, label, 0, "", 0, new List<string>(), "", PoolVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolVarianceStatus next)
    {
        switch (Status)
        {
            case PoolVarianceStatus.Created:
                switch (next)
                {
                    case PoolVarianceStatus.Validated:
                    case PoolVarianceStatus.Retired:
                        return true;
                }
                return false;
            case PoolVarianceStatus.Validated:
                switch (next)
                {
                    case PoolVarianceStatus.Published:
                        return true;
                }
                return false;
            case PoolVarianceStatus.Published:
                return next == PoolVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PoolVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pool variance"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)UpdatedAt + (double)Revision, 2);
    }

    public PoolVariance Copy()
    {
        return new PoolVariance(Code, Label, Revision, Locale, UpdatedAt, Tags, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
