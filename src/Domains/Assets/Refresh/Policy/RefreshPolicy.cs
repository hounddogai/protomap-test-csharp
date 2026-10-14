namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh policy is in its life cycle.</summary>
public enum RefreshPolicyStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A refresh policy. Its code is unique within a tenant.</summary>
public sealed class RefreshPolicy(string code, string label, string checksum, bool active, double rating, string unit, double ceiling, int updatedAt, string skill, int endsAt, RefreshPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Skill { get; set; } = skill;
    public int EndsAt { get; set; } = endsAt;
    public RefreshPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a refresh policy in status draft.</summary>
    public static RefreshPolicy NewRefreshPolicy(string code, string label)
    {
        return new RefreshPolicy(code, label, "", false, 0.0, "", 0.0, 0, "", 0, RefreshPolicyStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshPolicyStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshPolicyStatus next)
    {
        switch (Status)
        {
            case RefreshPolicyStatus.Draft:
                return next == RefreshPolicyStatus.Active || next == RefreshPolicyStatus.Closed;
            case RefreshPolicyStatus.Pending:
                return next == RefreshPolicyStatus.Active;
            case RefreshPolicyStatus.Active:
                switch (next)
                {
                    case RefreshPolicyStatus.Closed:
                        return true;
                }
                return false;
            case RefreshPolicyStatus.Suspended:
                switch (next)
                {
                    case RefreshPolicyStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)UpdatedAt + Rating, 2);
    }

    public RefreshPolicy Copy()
    {
        return new RefreshPolicy(
            Code,
            Label,
            Checksum,
            Active,
            Rating,
            Unit,
            Ceiling,
            UpdatedAt,
            Skill,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
