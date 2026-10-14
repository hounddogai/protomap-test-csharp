namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay band baseline is in its life cycle.</summary>
public enum ScheduledPayBandBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scheduled pay band baseline and its current status.</summary>
public sealed class ScheduledPayBandBaseline(string code, string title, double rate, double score, string employeeId, List<string> tags, double margin, string externalRef, double weight, ScheduledPayBandBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public double Score { get; set; } = score;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public double Margin { get; set; } = margin;
    public string ExternalRef { get; set; } = externalRef;
    public double Weight { get; set; } = weight;
    public ScheduledPayBandBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay band baseline in status draft.</summary>
    public static ScheduledPayBandBaseline NewScheduledPayBandBaseline(string code, string title)
    {
        return new ScheduledPayBandBaseline(
            code,
            title,
            0.0,
            0.0,
            "",
            new List<string>(),
            0.0,
            "",
            0.0,
            ScheduledPayBandBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayBandBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayBandBaselineStatus next)
    {
        switch (Status)
        {
            case ScheduledPayBandBaselineStatus.Draft:
                return next == ScheduledPayBandBaselineStatus.Suspended || next == ScheduledPayBandBaselineStatus.Closed;
            case ScheduledPayBandBaselineStatus.Pending:
                switch (next)
                {
                    case ScheduledPayBandBaselineStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledPayBandBaselineStatus.Active:
                switch (next)
                {
                    case ScheduledPayBandBaselineStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledPayBandBaselineStatus.Suspended:
                return next == ScheduledPayBandBaselineStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledPayBandBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay band baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double EffectiveLoad()
    {
        return Math.Round(Weight - Margin, 2);
    }

    public ScheduledPayBandBaseline Copy()
    {
        return new ScheduledPayBandBaseline(
            Code,
            Title,
            Rate,
            Score,
            EmployeeId,
            Tags,
            Margin,
            ExternalRef,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
