namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work entry is in its life cycle.</summary>
public enum StatementOfWorkEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One statement of work entry, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkEntry(string code, string label, double floorValue, string checksum, string managerId, string description, StatementOfWorkEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Checksum { get; set; } = checksum;
    public string ManagerId { get; set; } = managerId;
    public string Description { get; set; } = description;
    public StatementOfWorkEntryStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work entry in status created.</summary>
    public static StatementOfWorkEntry NewStatementOfWorkEntry(string code, string label)
    {
        return new StatementOfWorkEntry(code, label, 0.0, "", "", "", StatementOfWorkEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkEntryStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkEntryStatus.Created:
                switch (next)
                {
                    case StatementOfWorkEntryStatus.Published:
                        return true;
                }
                return false;
            case StatementOfWorkEntryStatus.Validated:
                switch (next)
                {
                    case StatementOfWorkEntryStatus.Retired:
                        return true;
                }
                return false;
            case StatementOfWorkEntryStatus.Published:
                switch (next)
                {
                    case StatementOfWorkEntryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"statement of work entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public StatementOfWorkEntry Copy()
    {
        return new StatementOfWorkEntry(Code, Label, FloorValue, Checksum, ManagerId, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
