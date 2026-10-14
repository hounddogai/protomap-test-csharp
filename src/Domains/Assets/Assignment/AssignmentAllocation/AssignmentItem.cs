namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment item is in its life cycle.</summary>
public enum AssignmentItemStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a assignment item and its current status.</summary>
public sealed class AssignmentItem(string code, string title, double hours, string checksum, bool visible, int updatedAt, double floorValue, string category, string label, string segment, string locale, AssignmentItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Checksum { get; set; } = checksum;
    public bool Visible { get; set; } = visible;
    public int UpdatedAt { get; set; } = updatedAt;
    public double FloorValue { get; set; } = floorValue;
    public string Category { get; set; } = category;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Locale { get; set; } = locale;
    public AssignmentItemStatus Status { get; set; } = status;

    /// <summary>Starts a assignment item in status created.</summary>
    public static AssignmentItem NewAssignmentItem(string code, string title)
    {
        return new AssignmentItem(code, title, 0.0, "", false, 0, 0.0, "", "", "", "", AssignmentItemStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentItemStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentItemStatus next)
    {
        switch (Status)
        {
            case AssignmentItemStatus.Created:
                return next == AssignmentItemStatus.Published || next == AssignmentItemStatus.Retired;
            case AssignmentItemStatus.Validated:
                return next == AssignmentItemStatus.Published || next == AssignmentItemStatus.Retired;
            case AssignmentItemStatus.Published:
                return next == AssignmentItemStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AssignmentItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)UpdatedAt * FloorValue, 2);
    }

    public AssignmentItem Copy()
    {
        return new AssignmentItem(
            Code,
            Title,
            Hours,
            Checksum,
            Visible,
            UpdatedAt,
            FloorValue,
            Category,
            Label,
            Segment,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
