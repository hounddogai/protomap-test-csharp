namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk mapping is in its life cycle.</summary>
public enum DeskMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a desk mapping and its current status.</summary>
public sealed class DeskMapping(string code, string title, double amount, string contactEmail, int tenureMonths, string category, DeskMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string ContactEmail { get; set; } = contactEmail;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Category { get; set; } = category;
    public DeskMappingStatus Status { get; set; } = status;

    /// <summary>Starts a desk mapping in status idle.</summary>
    public static DeskMapping NewDeskMapping(string code, string title)
    {
        return new DeskMapping(code, title, 0.0, "", 0, "", DeskMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskMappingStatus next)
    {
        switch (Status)
        {
            case DeskMappingStatus.Idle:
                return next == DeskMappingStatus.Stopped;
            case DeskMappingStatus.Warming:
                switch (next)
                {
                    case DeskMappingStatus.Stopped:
                        return true;
                }
                return false;
            case DeskMappingStatus.Ready:
                switch (next)
                {
                    case DeskMappingStatus.Draining:
                        return true;
                }
                return false;
            case DeskMappingStatus.Draining:
                return next == DeskMappingStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DeskMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"desk mapping"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Amount - (double)TenureMonths, 2);
    }

    public DeskMapping Copy()
    {
        return new DeskMapping(Code, Title, Amount, ContactEmail, TenureMonths, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
