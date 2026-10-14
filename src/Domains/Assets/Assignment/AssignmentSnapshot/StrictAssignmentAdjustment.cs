namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict assignment adjustment is in its life cycle.</summary>
public enum StrictAssignmentAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A strict assignment adjustment. Its code is unique within a tenant.</summary>
public sealed class StrictAssignmentAdjustment(string code, string label, string grade, string name, string description, double floorValue, string phone, Dictionary<string, string> attributes, int startsAt, double threshold, double rating, StrictAssignmentAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public double Threshold { get; set; } = threshold;
    public double Rating { get; set; } = rating;
    public StrictAssignmentAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a strict assignment adjustment in status requested.</summary>
    public static StrictAssignmentAdjustment NewStrictAssignmentAdjustment(string code, string label)
    {
        return new StrictAssignmentAdjustment(
            code,
            label,
            "",
            "",
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            0,
            0.0,
            0.0,
            StrictAssignmentAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAssignmentAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAssignmentAdjustmentStatus next)
    {
        switch (Status)
        {
            case StrictAssignmentAdjustmentStatus.Requested:
                return next == StrictAssignmentAdjustmentStatus.Granted;
            case StrictAssignmentAdjustmentStatus.Granted:
                return next == StrictAssignmentAdjustmentStatus.Expired;
            case StrictAssignmentAdjustmentStatus.Revoked:
                return next == StrictAssignmentAdjustmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictAssignmentAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict assignment adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double EffectiveValue()
    {
        return Math.Round(Threshold * FloorValue, 2);
    }

    public StrictAssignmentAdjustment Copy()
    {
        return new StrictAssignmentAdjustment(
            Code,
            Label,
            Grade,
            Name,
            Description,
            FloorValue,
            Phone,
            Attributes,
            StartsAt,
            Threshold,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
