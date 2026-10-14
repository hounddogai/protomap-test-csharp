namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path override is in its life cycle.</summary>
public enum CareerPathOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A career path override. Its code is unique within a tenant.</summary>
public sealed class CareerPathOverride(string code, string name, bool active, int createdAt, string salaryBand, Dictionary<string, string> attributes, string channel, int limit, double fte, int retries, CareerPathOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public int CreatedAt { get; set; } = createdAt;
    public string SalaryBand { get; set; } = salaryBand;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Channel { get; set; } = channel;
    public int Limit { get; set; } = limit;
    public double Fte { get; set; } = fte;
    public int Retries { get; set; } = retries;
    public CareerPathOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a career path override in status requested.</summary>
    public static CareerPathOverride NewCareerPathOverride(string code, string name)
    {
        return new CareerPathOverride(
            code,
            name,
            false,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            0,
            CareerPathOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathOverrideStatus next)
    {
        switch (Status)
        {
            case CareerPathOverrideStatus.Requested:
                switch (next)
                {
                    case CareerPathOverrideStatus.Granted:
                    case CareerPathOverrideStatus.Expired:
                        return true;
                }
                return false;
            case CareerPathOverrideStatus.Granted:
                return next == CareerPathOverrideStatus.Expired;
            case CareerPathOverrideStatus.Revoked:
                return next == CareerPathOverrideStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CareerPathOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveVolume()
    {
        return Math.Round(Fte * (double)Limit, 2);
    }

    public CareerPathOverride Copy()
    {
        return new CareerPathOverride(
            Code,
            Name,
            Active,
            CreatedAt,
            SalaryBand,
            Attributes,
            Channel,
            Limit,
            Fte,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
