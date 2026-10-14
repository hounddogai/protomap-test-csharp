namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor request is in its life cycle.</summary>
public enum VisitorRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A visitor request as the visitor team keeps it.</summary>
public sealed class VisitorRequest(string code, string label, double weight, int sequence, string managerId, VisitorRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public int Sequence { get; set; } = sequence;
    public string ManagerId { get; set; } = managerId;
    public VisitorRequestStatus Status { get; set; } = status;

    /// <summary>Starts a visitor request in status draft.</summary>
    public static VisitorRequest NewVisitorRequest(string code, string label)
    {
        return new VisitorRequest(code, label, 0.0, 0, "", VisitorRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorRequestStatus next)
    {
        switch (Status)
        {
            case VisitorRequestStatus.Draft:
                switch (next)
                {
                    case VisitorRequestStatus.Active:
                    case VisitorRequestStatus.Closed:
                        return true;
                }
                return false;
            case VisitorRequestStatus.Pending:
                return next == VisitorRequestStatus.Closed;
            case VisitorRequestStatus.Active:
                switch (next)
                {
                    case VisitorRequestStatus.Suspended:
                    case VisitorRequestStatus.Closed:
                        return true;
                }
                return false;
            case VisitorRequestStatus.Suspended:
                return next == VisitorRequestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(VisitorRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Sequence + Weight, 2);
    }

    public VisitorRequest Copy()
    {
        return new VisitorRequest(Code, Label, Weight, Sequence, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
