//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/WinWeed/Runtime/Editor/Input/WeedEditorInput.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Hsinpa.Winweed.EditorCode
{
    public partial class @WeedEditorInput: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @WeedEditorInput()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""WeedEditorInput"",
    ""maps"": [
        {
            ""name"": ""Editor"",
            ""id"": ""9c73c058-88d4-4954-abd2-d9788e2e6715"",
            ""actions"": [
                {
                    ""name"": ""Switch_State"",
                    ""type"": ""Button"",
                    ""id"": ""3ccaa95c-9912-4939-be7a-73ca5c25e39c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""9da07e07-6a9c-48b3-b769-9ceb3b42d71c"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Switch_State"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Editor
            m_Editor = asset.FindActionMap("Editor", throwIfNotFound: true);
            m_Editor_Switch_State = m_Editor.FindAction("Switch_State", throwIfNotFound: true);
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

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Editor
        private readonly InputActionMap m_Editor;
        private List<IEditorActions> m_EditorActionsCallbackInterfaces = new List<IEditorActions>();
        private readonly InputAction m_Editor_Switch_State;
        public struct EditorActions
        {
            private @WeedEditorInput m_Wrapper;
            public EditorActions(@WeedEditorInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @Switch_State => m_Wrapper.m_Editor_Switch_State;
            public InputActionMap Get() { return m_Wrapper.m_Editor; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(EditorActions set) { return set.Get(); }
            public void AddCallbacks(IEditorActions instance)
            {
                if (instance == null || m_Wrapper.m_EditorActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_EditorActionsCallbackInterfaces.Add(instance);
                @Switch_State.started += instance.OnSwitch_State;
                @Switch_State.performed += instance.OnSwitch_State;
                @Switch_State.canceled += instance.OnSwitch_State;
            }

            private void UnregisterCallbacks(IEditorActions instance)
            {
                @Switch_State.started -= instance.OnSwitch_State;
                @Switch_State.performed -= instance.OnSwitch_State;
                @Switch_State.canceled -= instance.OnSwitch_State;
            }

            public void RemoveCallbacks(IEditorActions instance)
            {
                if (m_Wrapper.m_EditorActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IEditorActions instance)
            {
                foreach (var item in m_Wrapper.m_EditorActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_EditorActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public EditorActions @Editor => new EditorActions(this);
        public interface IEditorActions
        {
            void OnSwitch_State(InputAction.CallbackContext context);
        }
    }
}