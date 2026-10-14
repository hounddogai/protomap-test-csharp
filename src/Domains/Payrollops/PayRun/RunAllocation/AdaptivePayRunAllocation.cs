namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive pay run allocation is in its life cycle.</summary>
public enum AdaptivePayRunAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a adaptive pay run allocation and its current status.</summary>
public sealed class AdaptivePayRunAllocation(string code, string title, List<string> tags, string summary, double rate, AdaptivePayRunAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Summary { get; set; } = summary;
    public double Rate { get; set; } = rate;
    public AdaptivePayRunAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive pay run allocation in status created.</summary>
    public static AdaptivePayRunAllocation NewAdaptivePayRunAllocation(string code, string title)
    {
        return new AdaptivePayRunAllocation(
            code,
            title,
            new List<string>(),
            "",
            0.0,
            AdaptivePayRunAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePayRunAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePayRunAllocationStatus next)
    {
        switch (Status)
        {
            case AdaptivePayRunAllocationStatus.Created:
                switch (next)
                {
                    case AdaptivePayRunAllocationStatus.Published:
                    case AdaptivePayRunAllocationStatus.Retired:
                        return true;
                }
                return false;
            case AdaptivePayRunAllocationStatus.Validated:
                return next == AdaptivePayRunAllocationStatus.Retired;
            case AdaptivePayRunAllocationStatus.Published:
                switch (next)
                {
                    case AdaptivePayRunAllocationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePayRunAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive pay run allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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
        return Rate;
    }

    public AdaptivePayRunAllocation Copy()
    {
        return new AdaptivePayRunAllocation(Code, Title, Tags, Summary, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
