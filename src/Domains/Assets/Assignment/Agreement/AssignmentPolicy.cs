namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment policy is in its life cycle.</summary>
public enum AssignmentPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a assignment policy and its current status.</summary>
public sealed class AssignmentPolicy(string code, string title, bool active, string label, double cost, double margin, int graceDays, double discount, int quantity, AssignmentPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public double Margin { get; set; } = margin;
    public int GraceDays { get; set; } = graceDays;
    public double Discount { get; set; } = discount;
    public int Quantity { get; set; } = quantity;
    public AssignmentPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a assignment policy in status created.</summary>
    public static AssignmentPolicy NewAssignmentPolicy(string code, string title)
    {
        return new AssignmentPolicy(code, title, false, "", 0.0, 0.0, 0, 0.0, 0, AssignmentPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentPolicyStatus next)
    {
        switch (Status)
        {
            case AssignmentPolicyStatus.Created:
                return next == AssignmentPolicyStatus.Validated || next == AssignmentPolicyStatus.Published;
            case AssignmentPolicyStatus.Validated:
                return next == AssignmentPolicyStatus.Published || next == AssignmentPolicyStatus.Retired;
            case AssignmentPolicyStatus.Published:
                return next == AssignmentPolicyStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AssignmentPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"assignment policy"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Margin * Discount, 2);
    }

    public AssignmentPolicy Copy()
    {
        return new AssignmentPolicy(Code, Title, Active, Label, Cost, Margin, GraceDays, Discount, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
