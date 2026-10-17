using ResourceMangementSystem.Domain.Enums;

namespace ResourceMangementSystem.Domain.Entities;

public sealed class ProjectRequirement
{
    public string SkillName { get; private set; }

    public ProficiencyLevel MinimumLevel { get; private set; }

    public ProjectRequirement(string skillName, ProficiencyLevel minimumLevel)
    {
        if (string.IsNullOrWhiteSpace(skillName))
        {
            throw new ArgumentException("Skill name is required.", nameof(skillName));
        }

        SkillName = skillName.Trim();
        MinimumLevel = minimumLevel;
    }
}
