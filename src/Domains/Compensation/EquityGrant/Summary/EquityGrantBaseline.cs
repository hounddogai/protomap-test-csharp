namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant baseline is in its life cycle.</summary>
public enum EquityGrantBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One equity grant baseline, with what the planners need to act on it.</summary>
public sealed class EquityGrantBaseline(string code, string name, string managerId, int leaveDays, string region, string department, Dictionary<string, string> attributes, string phone, EquityGrantBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public string Region { get; set; } = region;
    public string Department { get; set; } = department;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Phone { get; set; } = phone;
    public EquityGrantBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant baseline in status created.</summary>
    public static EquityGrantBaseline NewEquityGrantBaseline(string code, string name)
    {
        return new EquityGrantBaseline(
            code,
            name,
            "",
            0,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            EquityGrantBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantBaselineStatus next)
    {
        switch (Status)
        {
            case EquityGrantBaselineStatus.Created:
                return next == EquityGrantBaselineStatus.Published;
            case EquityGrantBaselineStatus.Validated:
                return next == EquityGrantBaselineStatus.Published || next == EquityGrantBaselineStatus.Retired;
            case EquityGrantBaselineStatus.Published:
                return next == EquityGrantBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(EquityGrantBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public EquityGrantBaseline Copy()
    {
        return new EquityGrantBaseline(
            Code,
            Name,
            ManagerId,
            LeaveDays,
            Region,
            Department,
            Attributes,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
