namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted retro pay notice is in its life cycle.</summary>
public enum WeightedRetroPayNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One weighted retro pay notice, with what the planners need to act on it.</summary>
public sealed class WeightedRetroPayNotice(string code, string label, int quantity, List<string> tags, string unit, string description, string phone, WeightedRetroPayNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public List<string> Tags { get; set; } = tags;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public string Phone { get; set; } = phone;
    public WeightedRetroPayNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a weighted retro pay notice in status proposed.</summary>
    public static WeightedRetroPayNotice NewWeightedRetroPayNotice(string code, string label)
    {
        return new WeightedRetroPayNotice(
            code,
            label,
            0,
            new List<string>(),
            "",
            "",
            "",
            WeightedRetroPayNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRetroPayNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRetroPayNoticeStatus next)
    {
        switch (Status)
        {
            case WeightedRetroPayNoticeStatus.Proposed:
                switch (next)
                {
                    case WeightedRetroPayNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedRetroPayNoticeStatus.Approved:
                return next == WeightedRetroPayNoticeStatus.Withdrawn;
            case WeightedRetroPayNoticeStatus.Rejected:
                switch (next)
                {
                    case WeightedRetroPayNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedRetroPayNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted retro pay notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted retro pay notice"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public WeightedRetroPayNotice Copy()
    {
        return new WeightedRetroPayNotice(Code, Label, Quantity, Tags, Unit, Description, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
