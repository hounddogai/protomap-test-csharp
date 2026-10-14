namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor preference is in its life cycle.</summary>
public enum VisitorPreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a visitor preference and its current status.</summary>
public sealed class VisitorPreference(string code, string title, int priority, int tenureMonths, int capacity, double weight, string description, string phone, bool pinned, string summary, string salaryBand, VisitorPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public double Weight { get; set; } = weight;
    public string Description { get; set; } = description;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public string Summary { get; set; } = summary;
    public string SalaryBand { get; set; } = salaryBand;
    public VisitorPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a visitor preference in status requested.</summary>
    public static VisitorPreference NewVisitorPreference(string code, string title)
    {
        return new VisitorPreference(
            code,
            title,
            0,
            0,
            0,
            0.0,
            "",
            "",
            false,
            "",
            "",
            VisitorPreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorPreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorPreferenceStatus next)
    {
        switch (Status)
        {
            case VisitorPreferenceStatus.Requested:
                return next == VisitorPreferenceStatus.Revoked || next == VisitorPreferenceStatus.Expired;
            case VisitorPreferenceStatus.Granted:
                return next == VisitorPreferenceStatus.Revoked;
            case VisitorPreferenceStatus.Revoked:
                switch (next)
                {
                    case VisitorPreferenceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Capacity - Weight, 2);
    }

    public VisitorPreference Copy()
    {
        return new VisitorPreference(
            Code,
            Title,
            Priority,
            TenureMonths,
            Capacity,
            Weight,
            Description,
            Phone,
            Pinned,
            Summary,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
