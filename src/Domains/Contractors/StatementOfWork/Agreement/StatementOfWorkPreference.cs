namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work preference is in its life cycle.</summary>
public enum StatementOfWorkPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a statement of work preference and its current status.</summary>
public sealed class StatementOfWorkPreference(string code, string name, string phone, double fte, int limit, int updatedAt, string salaryBand, bool archived, string jobCode, StatementOfWorkPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public double Fte { get; set; } = fte;
    public int Limit { get; set; } = limit;
    public int UpdatedAt { get; set; } = updatedAt;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Archived { get; set; } = archived;
    public string JobCode { get; set; } = jobCode;
    public StatementOfWorkPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work preference in status created.</summary>
    public static StatementOfWorkPreference NewStatementOfWorkPreference(string code, string name)
    {
        return new StatementOfWorkPreference(
            code,
            name,
            "",
            0.0,
            0,
            0,
            "",
            false,
            "",
            StatementOfWorkPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkPreferenceStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkPreferenceStatus.Created:
                return next == StatementOfWorkPreferenceStatus.Retired;
            case StatementOfWorkPreferenceStatus.Validated:
                return next == StatementOfWorkPreferenceStatus.Published || next == StatementOfWorkPreferenceStatus.Retired;
            case StatementOfWorkPreferenceStatus.Published:
                switch (next)
                {
                    case StatementOfWorkPreferenceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"statement of work preference"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Fte - (double)Limit, 2);
    }

    public StatementOfWorkPreference Copy()
    {
        return new StatementOfWorkPreference(
            Code,
            Name,
            Phone,
            Fte,
            Limit,
            UpdatedAt,
            SalaryBand,
            Archived,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
