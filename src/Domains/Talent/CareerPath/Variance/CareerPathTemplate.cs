namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path template is in its life cycle.</summary>
public enum CareerPathTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A career path template. Its code is unique within a tenant.</summary>
public sealed class CareerPathTemplate(string code, string name, double rate, int windowDays, int createdAt, CareerPathTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public int WindowDays { get; set; } = windowDays;
    public int CreatedAt { get; set; } = createdAt;
    public CareerPathTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a career path template in status requested.</summary>
    public static CareerPathTemplate NewCareerPathTemplate(string code, string name)
    {
        return new CareerPathTemplate(code, name, 0.0, 0, 0, CareerPathTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathTemplateStatus next)
    {
        switch (Status)
        {
            case CareerPathTemplateStatus.Requested:
                return next == CareerPathTemplateStatus.Expired;
            case CareerPathTemplateStatus.Granted:
                switch (next)
                {
                    case CareerPathTemplateStatus.Expired:
                        return true;
                }
                return false;
            case CareerPathTemplateStatus.Revoked:
                switch (next)
                {
                    case CareerPathTemplateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"career path template"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Rate * (double)CreatedAt, 2);
    }

    public CareerPathTemplate Copy()
    {
        return new CareerPathTemplate(Code, Name, Rate, WindowDays, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
