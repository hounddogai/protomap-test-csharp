namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool assignment is in its life cycle.</summary>
public enum BonusPoolAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A bonus pool assignment as the bonus pool team keeps it.</summary>
public sealed class BonusPoolAssignment(string code, string title, string phone, int capacity, Dictionary<string, string> attributes, double maximum, double discount, BonusPoolAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public int Capacity { get; set; } = capacity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Maximum { get; set; } = maximum;
    public double Discount { get; set; } = discount;
    public BonusPoolAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool assignment in status new.</summary>
    public static BonusPoolAssignment NewBonusPoolAssignment(string code, string title)
    {
        return new BonusPoolAssignment(
            code,
            title,
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            BonusPoolAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolAssignmentStatus next)
    {
        switch (Status)
        {
            case BonusPoolAssignmentStatus.New:
                return next == BonusPoolAssignmentStatus.Queued || next == BonusPoolAssignmentStatus.Done;
            case BonusPoolAssignmentStatus.Queued:
                return next == BonusPoolAssignmentStatus.Running;
            case BonusPoolAssignmentStatus.Running:
                switch (next)
                {
                    case BonusPoolAssignmentStatus.Done:
                    case BonusPoolAssignmentStatus.Failed:
                        return true;
                }
                return false;
            case BonusPoolAssignmentStatus.Done:
                return next == BonusPoolAssignmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BonusPoolAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double AdjustedLoad()
    {
        return Math.Round(Discount * (double)Capacity, 2);
    }

    public BonusPoolAssignment Copy()
    {
        return new BonusPoolAssignment(Code, Title, Phone, Capacity, Attributes, Maximum, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
