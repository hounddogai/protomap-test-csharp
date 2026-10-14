namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path manifest is in its life cycle.</summary>
public enum CareerPathManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A career path manifest. Its code is unique within a tenant.</summary>
public sealed class CareerPathManifest(string code, string title, double amount, double ratio, string phone, CareerPathManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public double Ratio { get; set; } = ratio;
    public string Phone { get; set; } = phone;
    public CareerPathManifestStatus Status { get; set; } = status;

    /// <summary>Starts a career path manifest in status planned.</summary>
    public static CareerPathManifest NewCareerPathManifest(string code, string title)
    {
        return new CareerPathManifest(code, title, 0.0, 0.0, "", CareerPathManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathManifestStatus next)
    {
        switch (Status)
        {
            case CareerPathManifestStatus.Planned:
                return next == CareerPathManifestStatus.Scheduled;
            case CareerPathManifestStatus.Scheduled:
                return next == CareerPathManifestStatus.InProgress || next == CareerPathManifestStatus.Cancelled;
            case CareerPathManifestStatus.InProgress:
                switch (next)
                {
                    case CareerPathManifestStatus.Completed:
                        return true;
                }
                return false;
            case CareerPathManifestStatus.Completed:
                return next == CareerPathManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CareerPathManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Amount + Ratio, 2);
    }

    public CareerPathManifest Copy()
    {
        return new CareerPathManifest(Code, Title, Amount, Ratio, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
