namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path profile is in its life cycle.</summary>
public enum CareerPathProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A career path profile as the career path team keeps it.</summary>
public sealed class CareerPathProfile(string code, string name, string phone, bool approved, double margin, int createdAt, double threshold, double rating, CareerPathProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public double Margin { get; set; } = margin;
    public int CreatedAt { get; set; } = createdAt;
    public double Threshold { get; set; } = threshold;
    public double Rating { get; set; } = rating;
    public CareerPathProfileStatus Status { get; set; } = status;

    /// <summary>Starts a career path profile in status new.</summary>
    public static CareerPathProfile NewCareerPathProfile(string code, string name)
    {
        return new CareerPathProfile(code, name, "", false, 0.0, 0, 0.0, 0.0, CareerPathProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathProfileStatus next)
    {
        switch (Status)
        {
            case CareerPathProfileStatus.New:
                switch (next)
                {
                    case CareerPathProfileStatus.Running:
                        return true;
                }
                return false;
            case CareerPathProfileStatus.Queued:
                return next == CareerPathProfileStatus.Running;
            case CareerPathProfileStatus.Running:
                return next == CareerPathProfileStatus.Done;
            case CareerPathProfileStatus.Done:
                switch (next)
                {
                    case CareerPathProfileStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"career path profile"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold - Margin, 2);
    }

    public CareerPathProfile Copy()
    {
        return new CareerPathProfile(Code, Name, Phone, Approved, Margin, CreatedAt, Threshold, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
