namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered position template is in its life cycle.</summary>
public enum TieredPositionTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One tiered position template, with what the planners need to act on it.</summary>
public sealed class TieredPositionTemplate(string code, string label, string channel, int windowDays, string checksum, bool archived, string unit, int leaveDays, TieredPositionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int WindowDays { get; set; } = windowDays;
    public string Checksum { get; set; } = checksum;
    public bool Archived { get; set; } = archived;
    public string Unit { get; set; } = unit;
    public int LeaveDays { get; set; } = leaveDays;
    public TieredPositionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a tiered position template in status draft.</summary>
    public static TieredPositionTemplate NewTieredPositionTemplate(string code, string label)
    {
        return new TieredPositionTemplate(code, label, "", 0, "", false, "", 0, TieredPositionTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPositionTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPositionTemplateStatus next)
    {
        switch (Status)
        {
            case TieredPositionTemplateStatus.Draft:
                switch (next)
                {
                    case TieredPositionTemplateStatus.Pending:
                    case TieredPositionTemplateStatus.Active:
                        return true;
                }
                return false;
            case TieredPositionTemplateStatus.Pending:
                switch (next)
                {
                    case TieredPositionTemplateStatus.Suspended:
                        return true;
                }
                return false;
            case TieredPositionTemplateStatus.Active:
                return next == TieredPositionTemplateStatus.Suspended || next == TieredPositionTemplateStatus.Closed;
            case TieredPositionTemplateStatus.Suspended:
                switch (next)
                {
                    case TieredPositionTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPositionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered position template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)WindowDays * (double)LeaveDays, 2);
    }

    public TieredPositionTemplate Copy()
    {
        return new TieredPositionTemplate(
            Code,
            Label,
            Channel,
            WindowDays,
            Checksum,
            Archived,
            Unit,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
