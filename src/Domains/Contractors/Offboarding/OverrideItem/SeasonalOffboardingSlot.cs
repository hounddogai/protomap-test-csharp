namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal offboarding slot is in its life cycle.</summary>
public enum SeasonalOffboardingSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One seasonal offboarding slot, with what the planners need to act on it.</summary>
public sealed class SeasonalOffboardingSlot(string code, string name, bool active, string jobCode, string contactEmail, string timezone, int sequence, int batchSize, double maximum, string salaryBand, int limit, SeasonalOffboardingSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public string Timezone { get; set; } = timezone;
    public int Sequence { get; set; } = sequence;
    public int BatchSize { get; set; } = batchSize;
    public double Maximum { get; set; } = maximum;
    public string SalaryBand { get; set; } = salaryBand;
    public int Limit { get; set; } = limit;
    public SeasonalOffboardingSlotStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal offboarding slot in status idle.</summary>
    public static SeasonalOffboardingSlot NewSeasonalOffboardingSlot(string code, string name)
    {
        return new SeasonalOffboardingSlot(
            code,
            name,
            false,
            "",
            "",
            "",
            0,
            0,
            0.0,
            "",
            0,
            SeasonalOffboardingSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalOffboardingSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalOffboardingSlotStatus next)
    {
        switch (Status)
        {
            case SeasonalOffboardingSlotStatus.Idle:
                return next == SeasonalOffboardingSlotStatus.Warming || next == SeasonalOffboardingSlotStatus.Draining;
            case SeasonalOffboardingSlotStatus.Warming:
                switch (next)
                {
                    case SeasonalOffboardingSlotStatus.Ready:
                    case SeasonalOffboardingSlotStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalOffboardingSlotStatus.Ready:
                switch (next)
                {
                    case SeasonalOffboardingSlotStatus.Draining:
                        return true;
                }
                return false;
            case SeasonalOffboardingSlotStatus.Draining:
                return next == SeasonalOffboardingSlotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalOffboardingSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal offboarding slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)BatchSize + (double)Limit, 2);
    }

    public SeasonalOffboardingSlot Copy()
    {
        return new SeasonalOffboardingSlot(
            Code,
            Name,
            Active,
            JobCode,
            ContactEmail,
            Timezone,
            Sequence,
            BatchSize,
            Maximum,
            SalaryBand,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
