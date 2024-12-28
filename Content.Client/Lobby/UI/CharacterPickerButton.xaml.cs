using System.Linq;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Lobby.UI;

/// <summary>
/// Holds character data on the side of the setup GUI.
/// </summary>
[GenerateTypedNameReferences]
public sealed partial class CharacterPickerButton : ContainerButton
{
    private IEntityManager _entManager;

    private EntityUid _previewDummy;

    /// <summary>
    /// Invoked if we should delete the attached character
    /// </summary>
    public event Action? OnDeletePressed;

    public CharacterPickerButton(
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        ButtonGroup group,
        ICharacterProfile profile,
        bool isSelected)
    {
        RobustXamlLoader.Load(this);
        _entManager = entityManager;
        AddStyleClass(StyleClassButton);
        ToggleMode = true;
        Group = group;
        var description = profile.Name;

        if (profile is not HumanoidCharacterProfile humanoid)
        {
            _previewDummy = entityManager.SpawnEntity(prototypeManager.Index<SpeciesPrototype>(SharedHumanoidAppearanceSystem.DefaultSpecies).DollPrototype, MapCoordinates.Nullspace);
        }
        else
        {
            _previewDummy = UserInterfaceManager.GetUIController<LobbyUIController>()
                .LoadProfileEntity(humanoid, null, true);

            var highPriorityJob = humanoid.JobPriorities.SingleOrDefault(p => p.Value == JobPriority.High).Key;
            if (highPriorityJob != default)
            {
                var jobName = prototypeManager.Index(highPriorityJob).LocalizedName;
                description = $"{description}\n{jobName}";
            }
        }

        Pressed = isSelected;
        DeleteButton.Visible = !isSelected;

        View.SetEntity(_previewDummy);
        DescriptionLabel.Text = description;

        ConfirmDeleteButton.OnPressed += _ =>
        {
            Parent?.RemoveChild(this);
            Parent?.RemoveChild(ConfirmDeleteButton);
            OnDeletePressed?.Invoke();
        };

        DeleteButton.OnPressed += _ =>
        {
            DeleteButton.Visible = false;
            ConfirmDeleteButton.Visible = true;
        };
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _entManager.DeleteEntity(_previewDummy);
        _previewDummy = default;
    }
}
