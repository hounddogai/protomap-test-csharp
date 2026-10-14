namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat bundle is in its life cycle.</summary>
public enum LicenseSeatBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One license seat bundle, with what the planners need to act on it.</summary>
public sealed class LicenseSeatBundle(string code, string title, double score, int batchSize, int expiresAt, int retries, double rating, Dictionary<string, string> attributes, string phone, string notes, string grade, LicenseSeatBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Retries { get; set; } = retries;
    public double Rating { get; set; } = rating;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Phone { get; set; } = phone;
    public string Notes { get; set; } = notes;
    public string Grade { get; set; } = grade;
    public LicenseSeatBundleStatus Status { get; set; } = status;

    /// <summary>Starts a license seat bundle in status planned.</summary>
    public static LicenseSeatBundle NewLicenseSeatBundle(string code, string title)
    {
        return new LicenseSeatBundle(
            code,
            title,
            0.0,
            0,
            0,
            0,
            0.0,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            LicenseSeatBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatBundleStatus next)
    {
        switch (Status)
        {
            case LicenseSeatBundleStatus.Planned:
                switch (next)
                {
                    case LicenseSeatBundleStatus.InProgress:
                        return true;
                }
                return false;
            case LicenseSeatBundleStatus.Scheduled:
                return next == LicenseSeatBundleStatus.Cancelled;
            case LicenseSeatBundleStatus.InProgress:
                return next == LicenseSeatBundleStatus.Cancelled;
            case LicenseSeatBundleStatus.Completed:
                switch (next)
                {
                    case LicenseSeatBundleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rating * Score, 2);
    }

    public LicenseSeatBundle Copy()
    {
        return new LicenseSeatBundle(
            Code,
            Title,
            Score,
            BatchSize,
            ExpiresAt,
            Retries,
            Rating,
            Attributes,
            Phone,
            Notes,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
