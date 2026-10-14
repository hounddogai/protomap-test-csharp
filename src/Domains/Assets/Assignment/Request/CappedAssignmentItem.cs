namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped assignment item is in its life cycle.</summary>
public enum CappedAssignmentItemStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a capped assignment item and its current status.</summary>
public sealed class CappedAssignmentItem(string code, string label, string department, string region, string costCenter, double score, CappedAssignmentItemStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public string Region { get; set; } = region;
    public string CostCenter { get; set; } = costCenter;
    public double Score { get; set; } = score;
    public CappedAssignmentItemStatus Status { get; set; } = status;

    /// <summary>Starts a capped assignment item in status created.</summary>
    public static CappedAssignmentItem NewCappedAssignmentItem(string code, string label)
    {
        return new CappedAssignmentItem(code, label, "", "", "", 0.0, CappedAssignmentItemStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAssignmentItemStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAssignmentItemStatus next)
    {
        switch (Status)
        {
            case CappedAssignmentItemStatus.Created:
                switch (next)
                {
                    case CappedAssignmentItemStatus.Validated:
                        return true;
                }
                return false;
            case CappedAssignmentItemStatus.Validated:
                switch (next)
                {
                    case CappedAssignmentItemStatus.Published:
                    case CappedAssignmentItemStatus.Retired:
                        return true;
                }
                return false;
            case CappedAssignmentItemStatus.Published:
                return next == CappedAssignmentItemStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedAssignmentItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped assignment item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped assignment item"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public CappedAssignmentItem Copy()
    {
        return new CappedAssignmentItem(Code, Label, Department, Region, CostCenter, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
