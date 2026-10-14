namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default reorg group is in its life cycle.</summary>
public enum DefaultReorgGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a default reorg group and its current status.</summary>
public sealed class DefaultReorgGroup(string code, string label, double fte, int updatedAt, double discount, DefaultReorgGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Discount { get; set; } = discount;
    public DefaultReorgGroupStatus Status { get; set; } = status;

    /// <summary>Starts a default reorg group in status created.</summary>
    public static DefaultReorgGroup NewDefaultReorgGroup(string code, string label)
    {
        return new DefaultReorgGroup(code, label, 0.0, 0, 0.0, DefaultReorgGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReorgGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReorgGroupStatus next)
    {
        switch (Status)
        {
            case DefaultReorgGroupStatus.Created:
                switch (next)
                {
                    case DefaultReorgGroupStatus.Retired:
                        return true;
                }
                return false;
            case DefaultReorgGroupStatus.Validated:
                return next == DefaultReorgGroupStatus.Published || next == DefaultReorgGroupStatus.Retired;
            case DefaultReorgGroupStatus.Published:
                return next == DefaultReorgGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultReorgGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default reorg group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)UpdatedAt + Discount, 2);
    }

    public DefaultReorgGroup Copy()
    {
        return new DefaultReorgGroup(Code, Label, Fte, UpdatedAt, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
