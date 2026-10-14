namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path group is in its life cycle.</summary>
public enum CareerPathGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A career path group. Its code is unique within a tenant.</summary>
public sealed class CareerPathGroup(string code, string label, string phone, Dictionary<string, string> attributes, int updatedAt, string currency, bool archived, int expiresAt, bool approved, int version, double ceiling, CareerPathGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Approved { get; set; } = approved;
    public int Version { get; set; } = version;
    public double Ceiling { get; set; } = ceiling;
    public CareerPathGroupStatus Status { get; set; } = status;

    /// <summary>Starts a career path group in status idle.</summary>
    public static CareerPathGroup NewCareerPathGroup(string code, string label)
    {
        return new CareerPathGroup(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            0,
            "",
            false,
            0,
            false,
            0,
            0.0,
            CareerPathGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathGroupStatus next)
    {
        switch (Status)
        {
            case CareerPathGroupStatus.Idle:
                return next == CareerPathGroupStatus.Warming || next == CareerPathGroupStatus.Ready;
            case CareerPathGroupStatus.Warming:
                switch (next)
                {
                    case CareerPathGroupStatus.Draining:
                        return true;
                }
                return false;
            case CareerPathGroupStatus.Ready:
                return next == CareerPathGroupStatus.Draining;
            case CareerPathGroupStatus.Draining:
                return next == CareerPathGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CareerPathGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"career path group"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round((double)Version - (double)UpdatedAt, 2);
    }

    public CareerPathGroup Copy()
    {
        return new CareerPathGroup(
            Code,
            Label,
            Phone,
            Attributes,
            UpdatedAt,
            Currency,
            Archived,
            ExpiresAt,
            Approved,
            Version,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
