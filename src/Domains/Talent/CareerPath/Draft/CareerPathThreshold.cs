namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path threshold is in its life cycle.</summary>
public enum CareerPathThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A career path threshold. Its code is unique within a tenant.</summary>
public sealed class CareerPathThreshold(string code, string label, bool approved, string badge, int createdAt, int endsAt, string summary, CareerPathThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public string Badge { get; set; } = badge;
    public int CreatedAt { get; set; } = createdAt;
    public int EndsAt { get; set; } = endsAt;
    public string Summary { get; set; } = summary;
    public CareerPathThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a career path threshold in status draft.</summary>
    public static CareerPathThreshold NewCareerPathThreshold(string code, string label)
    {
        return new CareerPathThreshold(code, label, false, "", 0, 0, "", CareerPathThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathThresholdStatus next)
    {
        switch (Status)
        {
            case CareerPathThresholdStatus.Draft:
                return next == CareerPathThresholdStatus.Pending || next == CareerPathThresholdStatus.Active;
            case CareerPathThresholdStatus.Pending:
                switch (next)
                {
                    case CareerPathThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case CareerPathThresholdStatus.Active:
                switch (next)
                {
                    case CareerPathThresholdStatus.Suspended:
                    case CareerPathThresholdStatus.Closed:
                        return true;
                }
                return false;
            case CareerPathThresholdStatus.Suspended:
                switch (next)
                {
                    case CareerPathThresholdStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)CreatedAt * (double)EndsAt, 2);
    }

    public CareerPathThreshold Copy()
    {
        return new CareerPathThreshold(Code, Label, Approved, Badge, CreatedAt, EndsAt, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
