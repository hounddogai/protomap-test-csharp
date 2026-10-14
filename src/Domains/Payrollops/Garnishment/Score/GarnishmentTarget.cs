namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment target is in its life cycle.</summary>
public enum GarnishmentTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One garnishment target, with what the planners need to act on it.</summary>
public sealed class GarnishmentTarget(string code, string title, List<string> tags, string country, int createdAt, double ratio, double rate, double maximum, string phone, string checksum, GarnishmentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public double Rate { get; set; } = rate;
    public double Maximum { get; set; } = maximum;
    public string Phone { get; set; } = phone;
    public string Checksum { get; set; } = checksum;
    public GarnishmentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment target in status idle.</summary>
    public static GarnishmentTarget NewGarnishmentTarget(string code, string title)
    {
        return new GarnishmentTarget(
            code,
            title,
            new List<string>(),
            "",
            0,
            0.0,
            0.0,
            0.0,
            "",
            "",
            GarnishmentTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentTargetStatus next)
    {
        switch (Status)
        {
            case GarnishmentTargetStatus.Idle:
                return next == GarnishmentTargetStatus.Draining || next == GarnishmentTargetStatus.Stopped;
            case GarnishmentTargetStatus.Warming:
                switch (next)
                {
                    case GarnishmentTargetStatus.Ready:
                    case GarnishmentTargetStatus.Draining:
                        return true;
                }
                return false;
            case GarnishmentTargetStatus.Ready:
                return next == GarnishmentTargetStatus.Draining;
            case GarnishmentTargetStatus.Draining:
                switch (next)
                {
                    case GarnishmentTargetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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
        return Math.Round(Maximum - (double)CreatedAt, 2);
    }

    public GarnishmentTarget Copy()
    {
        return new GarnishmentTarget(
            Code,
            Title,
            Tags,
            Country,
            CreatedAt,
            Ratio,
            Rate,
            Maximum,
            Phone,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
