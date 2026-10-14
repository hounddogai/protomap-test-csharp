namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path agreement is in its life cycle.</summary>
public enum CareerPathAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A career path agreement as the career path team keeps it.</summary>
public sealed class CareerPathAgreement(string code, string name, bool visible, int capacity, string region, string label, double hours, int attempts, string description, double discount, bool pinned, CareerPathAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public int Capacity { get; set; } = capacity;
    public string Region { get; set; } = region;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public double Discount { get; set; } = discount;
    public bool Pinned { get; set; } = pinned;
    public CareerPathAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a career path agreement in status draft.</summary>
    public static CareerPathAgreement NewCareerPathAgreement(string code, string name)
    {
        return new CareerPathAgreement(
            code,
            name,
            false,
            0,
            "",
            "",
            0.0,
            0,
            "",
            0.0,
            false,
            CareerPathAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathAgreementStatus next)
    {
        switch (Status)
        {
            case CareerPathAgreementStatus.Draft:
                return next == CareerPathAgreementStatus.Pending || next == CareerPathAgreementStatus.Active;
            case CareerPathAgreementStatus.Pending:
                return next == CareerPathAgreementStatus.Active || next == CareerPathAgreementStatus.Suspended;
            case CareerPathAgreementStatus.Active:
                switch (next)
                {
                    case CareerPathAgreementStatus.Closed:
                        return true;
                }
                return false;
            case CareerPathAgreementStatus.Suspended:
                return next == CareerPathAgreementStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CareerPathAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Capacity - Discount, 2);
    }

    public CareerPathAgreement Copy()
    {
        return new CareerPathAgreement(
            Code,
            Name,
            Visible,
            Capacity,
            Region,
            Label,
            Hours,
            Attempts,
            Description,
            Discount,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
