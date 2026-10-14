namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary deduction budget is in its life cycle.</summary>
public enum PrimaryDeductionBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a primary deduction budget and its current status.</summary>
public sealed class PrimaryDeductionBudget(string code, string name, string source, double rate, int createdAt, string country, double floorValue, string skill, PrimaryDeductionBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public double Rate { get; set; } = rate;
    public int CreatedAt { get; set; } = createdAt;
    public string Country { get; set; } = country;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public PrimaryDeductionBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a primary deduction budget in status created.</summary>
    public static PrimaryDeductionBudget NewPrimaryDeductionBudget(string code, string name)
    {
        return new PrimaryDeductionBudget(code, name, "", 0.0, 0, "", 0.0, "", PrimaryDeductionBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryDeductionBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryDeductionBudgetStatus next)
    {
        switch (Status)
        {
            case PrimaryDeductionBudgetStatus.Created:
                switch (next)
                {
                    case PrimaryDeductionBudgetStatus.Validated:
                    case PrimaryDeductionBudgetStatus.Retired:
                        return true;
                }
                return false;
            case PrimaryDeductionBudgetStatus.Validated:
                return next == PrimaryDeductionBudgetStatus.Published || next == PrimaryDeductionBudgetStatus.Retired;
            case PrimaryDeductionBudgetStatus.Published:
                return next == PrimaryDeductionBudgetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryDeductionBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary deduction budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary deduction budget"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rate - FloorValue, 2);
    }

    public PrimaryDeductionBudget Copy()
    {
        return new PrimaryDeductionBudget(Code, Name, Source, Rate, CreatedAt, Country, FloorValue, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
