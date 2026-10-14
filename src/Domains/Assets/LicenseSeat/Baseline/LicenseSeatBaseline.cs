namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat baseline is in its life cycle.</summary>
public enum LicenseSeatBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A license seat baseline as the license seat team keeps it.</summary>
public sealed class LicenseSeatBaseline(string code, string title, string segment, string team, string summary, List<string> tags, double amount, LicenseSeatBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public string Team { get; set; } = team;
    public string Summary { get; set; } = summary;
    public List<string> Tags { get; set; } = tags;
    public double Amount { get; set; } = amount;
    public LicenseSeatBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a license seat baseline in status requested.</summary>
    public static LicenseSeatBaseline NewLicenseSeatBaseline(string code, string title)
    {
        return new LicenseSeatBaseline(
            code,
            title,
            "",
            "",
            "",
            new List<string>(),
            0.0,
            LicenseSeatBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatBaselineStatus next)
    {
        switch (Status)
        {
            case LicenseSeatBaselineStatus.Requested:
                return next == LicenseSeatBaselineStatus.Revoked || next == LicenseSeatBaselineStatus.Expired;
            case LicenseSeatBaselineStatus.Granted:
                switch (next)
                {
                    case LicenseSeatBaselineStatus.Revoked:
                    case LicenseSeatBaselineStatus.Expired:
                        return true;
                }
                return false;
            case LicenseSeatBaselineStatus.Revoked:
                return next == LicenseSeatBaselineStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LicenseSeatBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"license seat baseline"} {Code}";
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

    public double MeasuredValue()
    {
        return Amount;
    }

    public LicenseSeatBaseline Copy()
    {
        return new LicenseSeatBaseline(Code, Title, Segment, Team, Summary, Tags, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
