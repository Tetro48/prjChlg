// GENERATED AUTOMATICALLY FROM 'Assets/Inputs.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Inputs : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Inputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Inputs"",
    ""maps"": [
        {
            ""name"": ""Game maps"",
            ""id"": ""94f4f7d2-6ee7-4e61-9a3a-91b3233b248b"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""a56601bd-502e-4770-8fc2-136638dedb16"",
                    ""expectedControlType"": ""Digital"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Counterclockwise"",
                    ""type"": ""Button"",
                    ""id"": ""fcc83a91-170d-48a4-882c-840d92c8be88"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Clockwise"",
                    ""type"": ""Button"",
                    ""id"": ""da2010e5-7209-450d-a26a-c29b44be5992"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Clockwise 2"",
                    ""type"": ""Button"",
                    ""id"": ""4b820601-d419-4422-ae27-e9e5cf8bcc62"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""UpsideDown"",
                    ""type"": ""Button"",
                    ""id"": ""404c7974-63fd-4e34-86cf-eeeb33a62d7c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Hold"",
                    ""type"": ""Button"",
                    ""id"": ""58394490-f9be-48bd-863b-2a7571ed01be"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""4ebbee2c-e26b-4bd4-b26e-fd75d3127f33"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Framestep"",
                    ""type"": ""Button"",
                    ""id"": ""c81b05b0-1a7b-4ec9-8adc-8db3aca77184"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Show Grade Score"",
                    ""type"": ""Button"",
                    ""id"": ""cf300265-2f9e-481f-bd7d-0a0d56df258c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""ab82148a-ff02-46c6-b27e-336af56096d4"",
                    ""path"": ""2DVector(mode=1)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a2c8f028-dede-438d-88df-957b06b6e215"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""ef31d3ea-e09f-4e5a-a894-aff5da19929d"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""62a1f76b-bbe7-4cae-bf0e-ce39b8c7f029"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""468ef2ec-97a6-4663-aa54-ecb86fdab466"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""2d63d6a3-b675-4466-ab8a-9dff779efbf0"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""95d8e7aa-637e-43fd-a444-4b52ed71cf01"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""48c36cdb-ec52-4683-9fad-d25ac53b8a2f"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""1a8633c7-b5a5-4787-9dd5-193afd41347d"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""99b5b9b0-e480-49ac-98c7-6075bcc84ae0"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""f1d29189-ac63-4ee9-9728-6882f0600842"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Counterclockwise"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3e676b54-e892-400b-a0f6-fdc0a007573a"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Counterclockwise"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""65ddbd91-1178-4d0e-8093-0a6905755c3c"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Clockwise"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""df416416-c146-44e6-87d8-cc555fb95e1b"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UpsideDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""13b8d397-6c57-4592-a05d-abbd99e3a4d3"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UpsideDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7b923eaf-fa6f-46ce-b0eb-ae1c17903346"",
                    ""path"": ""<Keyboard>/f1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e0a7648e-18c4-4111-a912-aaad85b92ef6"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cd30c36d-ea7b-47fc-8424-54beb12005a8"",
                    ""path"": ""<Keyboard>/n"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Framestep"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80d686b1-4b82-4628-af71-155919d5a9ae"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Framestep"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7dac5d4b-bde6-4215-9211-c601737ccaa6"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Clockwise 2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c509678b-121a-484f-ae49-7c775d5230fd"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Clockwise 2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8e856f33-89d7-4d4c-b9db-5f6182b7cc96"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hold"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6068c7fa-fbd1-4b5e-91ce-742d06804935"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hold"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""91caf664-a0d4-41cd-9fc4-8d300cdbc200"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Clockwise"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""57416ef8-3c1b-4e02-acbf-4a2b0d9183e8"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Show Grade Score"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Game maps
        m_Gamemaps = asset.FindActionMap("Game maps", throwIfNotFound: true);
        m_Gamemaps_Movement = m_Gamemaps.FindAction("Movement", throwIfNotFound: true);
        m_Gamemaps_Counterclockwise = m_Gamemaps.FindAction("Counterclockwise", throwIfNotFound: true);
        m_Gamemaps_Clockwise = m_Gamemaps.FindAction("Clockwise", throwIfNotFound: true);
        m_Gamemaps_Clockwise2 = m_Gamemaps.FindAction("Clockwise 2", throwIfNotFound: true);
        m_Gamemaps_UpsideDown = m_Gamemaps.FindAction("UpsideDown", throwIfNotFound: true);
        m_Gamemaps_Hold = m_Gamemaps.FindAction("Hold", throwIfNotFound: true);
        m_Gamemaps_Pause = m_Gamemaps.FindAction("Pause", throwIfNotFound: true);
        m_Gamemaps_Framestep = m_Gamemaps.FindAction("Framestep", throwIfNotFound: true);
        m_Gamemaps_ShowGradeScore = m_Gamemaps.FindAction("Show Grade Score", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Game maps
    private readonly InputActionMap m_Gamemaps;
    private IGamemapsActions m_GamemapsActionsCallbackInterface;
    private readonly InputAction m_Gamemaps_Movement;
    private readonly InputAction m_Gamemaps_Counterclockwise;
    private readonly InputAction m_Gamemaps_Clockwise;
    private readonly InputAction m_Gamemaps_Clockwise2;
    private readonly InputAction m_Gamemaps_UpsideDown;
    private readonly InputAction m_Gamemaps_Hold;
    private readonly InputAction m_Gamemaps_Pause;
    private readonly InputAction m_Gamemaps_Framestep;
    private readonly InputAction m_Gamemaps_ShowGradeScore;
    public struct GamemapsActions
    {
        private @Inputs m_Wrapper;
        public GamemapsActions(@Inputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_Gamemaps_Movement;
        public InputAction @Counterclockwise => m_Wrapper.m_Gamemaps_Counterclockwise;
        public InputAction @Clockwise => m_Wrapper.m_Gamemaps_Clockwise;
        public InputAction @Clockwise2 => m_Wrapper.m_Gamemaps_Clockwise2;
        public InputAction @UpsideDown => m_Wrapper.m_Gamemaps_UpsideDown;
        public InputAction @Hold => m_Wrapper.m_Gamemaps_Hold;
        public InputAction @Pause => m_Wrapper.m_Gamemaps_Pause;
        public InputAction @Framestep => m_Wrapper.m_Gamemaps_Framestep;
        public InputAction @ShowGradeScore => m_Wrapper.m_Gamemaps_ShowGradeScore;
        public InputActionMap Get() { return m_Wrapper.m_Gamemaps; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GamemapsActions set) { return set.Get(); }
        public void SetCallbacks(IGamemapsActions instance)
        {
            if (m_Wrapper.m_GamemapsActionsCallbackInterface != null)
            {
                @Movement.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnMovement;
                @Counterclockwise.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnCounterclockwise;
                @Counterclockwise.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnCounterclockwise;
                @Counterclockwise.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnCounterclockwise;
                @Clockwise.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnClockwise;
                @Clockwise.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnClockwise;
                @Clockwise.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnClockwise;
                @Clockwise2.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnClockwise2;
                @Clockwise2.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnClockwise2;
                @Clockwise2.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnClockwise2;
                @UpsideDown.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnUpsideDown;
                @UpsideDown.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnUpsideDown;
                @UpsideDown.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnUpsideDown;
                @Hold.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnHold;
                @Hold.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnHold;
                @Hold.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnHold;
                @Pause.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnPause;
                @Framestep.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnFramestep;
                @Framestep.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnFramestep;
                @Framestep.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnFramestep;
                @ShowGradeScore.started -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnShowGradeScore;
                @ShowGradeScore.performed -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnShowGradeScore;
                @ShowGradeScore.canceled -= m_Wrapper.m_GamemapsActionsCallbackInterface.OnShowGradeScore;
            }
            m_Wrapper.m_GamemapsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
                @Counterclockwise.started += instance.OnCounterclockwise;
                @Counterclockwise.performed += instance.OnCounterclockwise;
                @Counterclockwise.canceled += instance.OnCounterclockwise;
                @Clockwise.started += instance.OnClockwise;
                @Clockwise.performed += instance.OnClockwise;
                @Clockwise.canceled += instance.OnClockwise;
                @Clockwise2.started += instance.OnClockwise2;
                @Clockwise2.performed += instance.OnClockwise2;
                @Clockwise2.canceled += instance.OnClockwise2;
                @UpsideDown.started += instance.OnUpsideDown;
                @UpsideDown.performed += instance.OnUpsideDown;
                @UpsideDown.canceled += instance.OnUpsideDown;
                @Hold.started += instance.OnHold;
                @Hold.performed += instance.OnHold;
                @Hold.canceled += instance.OnHold;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @Framestep.started += instance.OnFramestep;
                @Framestep.performed += instance.OnFramestep;
                @Framestep.canceled += instance.OnFramestep;
                @ShowGradeScore.started += instance.OnShowGradeScore;
                @ShowGradeScore.performed += instance.OnShowGradeScore;
                @ShowGradeScore.canceled += instance.OnShowGradeScore;
            }
        }
    }
    public GamemapsActions @Gamemaps => new GamemapsActions(this);
    public interface IGamemapsActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnCounterclockwise(InputAction.CallbackContext context);
        void OnClockwise(InputAction.CallbackContext context);
        void OnClockwise2(InputAction.CallbackContext context);
        void OnUpsideDown(InputAction.CallbackContext context);
        void OnHold(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnFramestep(InputAction.CallbackContext context);
        void OnShowGradeScore(InputAction.CallbackContext context);
    }
}
