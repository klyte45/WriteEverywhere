using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public abstract class WTSBaseParamsTab<T> : IGUITab<T>
    {
        internal enum State
        {
            List,
            GettingImage,
            GettingFolder,
            GettingText,
            GettingAny
        }

        public abstract Texture TabIcon { get; }
        private Vector2 m_leftPanelScroll;
        private Vector2 m_rightPanelScroll;
        private int m_currentEditingParam = 0;

        internal readonly string Text = Color.green.ToRGB();
        internal readonly string Image = Color.cyan.ToRGB();
        internal readonly string Folder = Color.yellow.ToRGB();
        internal readonly string Any = Color.blue.ToRGB();

        internal readonly WTSParameterVariableEditor<T> TextVarEditor = new WTSParameterVariableEditor<T>();
        internal readonly WTSParameterImageEditor<T> ImageVarEditor = new WTSParameterImageEditor<T>();
        internal readonly WTSParameterFolderEditor<T> FolderVarEditor = new WTSParameterFolderEditor<T>();
        internal readonly WTSParameterAnyEditor<T> AnyVarEditor = new WTSParameterAnyEditor<T>();

        #region Basic Behavior
        internal bool ShowTabsOnTop() => CurrentState == State.List;

        public void Reset()
        {
            CurrentState = State.List;
            m_currentEditingParam = 0;
        }
        public bool DrawArea(Vector2 areaRect, ref T item, int _, bool isEditable)
        {
            switch (CurrentState)
            {
                case State.List:
                    DrawListing(areaRect, item, isEditable);
                    break;
                case State.GettingImage:
                    DrawImagePicker(item, areaRect);
                    break;
                case State.GettingFolder:
                    DrawFolderPicker(item, areaRect);
                    break;
                case State.GettingText:
                    DrawVariablePicker(item, areaRect);
                    break;
                case State.GettingAny:
                    DrawAnyPicker(item, areaRect);
                    break;
            }
            return false;
        }

        private void DrawImagePicker(T item, Vector2 areaRect) => DrawSelectorView(item, areaRect, ImageVarEditor);

        private void DrawFolderPicker(T item, Vector2 areaRect) => DrawSelectorView(item, areaRect, FolderVarEditor);

        private void DrawVariablePicker(T item, Vector2 areaRect) => DrawSelectorView(item, areaRect, TextVarEditor, IsVariable);
        private void DrawAnyPicker(T item, Vector2 areaRect) => DrawSelectorView(item, areaRect, AnyVarEditor, IsVariable || !IsTextVariable);

        #endregion

        #region Param editor commons

        private void OnClearSearch(IWTSParameterEditor<T> paramEditor, bool isText)
        {
            SelectedFolder = null;
            SelectedValue = null;
            m_searchResult.Value = new string[0];
            if (isText)
            {
                SearchText = "";
                if (IsVariable)
                {
                    RestartFilterCoroutine(paramEditor);
                }
            }
            else
            {
                RestartFilterCoroutine(paramEditor);
            }
        }
        private void DrawSelectorView(T item, Vector2 areaRect, IWTSParameterEditor<T> paramEditor, bool showRightPanel = true)
        {
            var topHeight = paramEditor.DrawTop(this, areaRect);
            using (new GUILayout.HorizontalScope())
            {
                using (var scroll = new GUILayout.ScrollViewScope(m_leftPanelScroll, false, true, GUILayout.Width(areaRect.x / 2), GUILayout.Height(areaRect.y - topHeight - 30)))
                {
                    paramEditor.DrawLeftPanel(this, areaRect);
                    m_leftPanelScroll = scroll.scrollPosition;
                };
                using (new GUILayout.VerticalScope(GUILayout.Width(areaRect.x / 2 - 20), GUILayout.Height(areaRect.y - topHeight - 30)))
                {
                    if (showRightPanel)
                    {
                        GUILayout.Label(GetCurrentParamString(), new GUIStyle(GUI.skin.label) { normal = new GUIStyleState() { textColor = Color.yellow } });
                        GUILayout.Space(8);
                        using (var scroll = new GUILayout.ScrollViewScope(m_rightPanelScroll))
                        {
                            paramEditor.DrawRightPanel(this, new Vector2(areaRect.x / 2 - 20, areaRect.y - topHeight - 50));
                            m_rightPanelScroll = scroll.scrollPosition;
                        }
                    }
                };
            };
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(Locale.Get("CANCEL"), GUILayout.Width(areaRect.x / 3)))
                {
                    CurrentState = State.List;
                }
                if (GUILayout.Button(Locale.Get("DEBUG_CLEAR"), GUILayout.Width(areaRect.x / 3)))
                {
                    OnClearSearch(paramEditor, paramEditor.IsText);
                }
                if (GUILayout.Button("OK"))
                {
                    GetBackToList(item, paramEditor);
                }
            };
        }

        internal string GetCurrentParamString()
        {
            switch (CurrentState)
            {
                case State.GettingImage:
                ImageVar:
                    return $"{(IsLocal ? "image" : "assetImage")}://{(SelectedFolder is null ? "" : (SelectedFolder == "" ? "<ROOT>" : SelectedFolder) + "/")}{SelectedValue}";
                case State.GettingFolder:
                    return $"{(IsLocal ? "folder" : "assetFolder")}://{(SelectedFolder == "" ? "<ROOT>" : SelectedFolder)}";
                case State.GettingText:
                TextVar:
                    return IsVariable ? $"var://{SelectedValue}" : SearchText;
                case State.GettingAny:
                    if (IsTextVariable) goto TextVar;
                    else goto ImageVar;
            }
            return null;
        }
        protected void GoToPicker(int key, TextContent targetContentType, TextParameterWrapper paramVal, T item)
        {
            IWTSParameterEditor<T> paramEditor;
            switch (targetContentType)
            {
                case TextContent.ParameterizedSpriteSingle:
                    SearchText = "";
                    IsLocal = paramVal?.IsLocal ?? true;
                    SelectedFolder = paramVal?.AtlasName.TrimToNull();
                    SelectedValue = paramVal?.TextOrSpriteValue;
                    CurrentState = State.GettingImage;
                    paramEditor = ImageVarEditor;
                    break;
                case TextContent.ParameterizedSpriteFolder:
                    SearchText = "";
                    IsLocal = paramVal?.IsLocal ?? true;
                    SelectedFolder = paramVal?.AtlasName.TrimToNull();
                    CurrentState = State.GettingFolder;
                    paramEditor = FolderVarEditor;
                    break;
                case TextContent.ParameterizedText:
                    IsVariable = paramVal?.ParamType == ParameterType.VARIABLE;
                    CurrentState = State.GettingText;
                    if (IsVariable)
                    {
                        SearchText = "";
                        SelectedValue = paramVal?.GetOriginalVariableParam();
                    }
                    else
                    {
                        SearchText = paramVal?.TextOrSpriteValue ?? "";
                        SelectedValue = null;
                    }
                    paramEditor = TextVarEditor;
                    break;
                case TextContent.Any:
                case TextContent.TextParameterSequence:
                default:
                    CurrentState = State.GettingAny;
                    if (IsVariable)
                    {
                        SearchText = "";
                        SelectedValue = paramVal?.GetOriginalVariableParam();
                    }
                    else
                    {
                        if (paramVal.ParamType.Equals(ParameterType.IMAGE))
                        {
                            SearchText = "";
                            IsLocal = paramVal.IsLocal;
                            SelectedFolder = paramVal?.AtlasName.TrimToNull();
                            SelectedValue = paramVal.TextOrSpriteValue;
                            CurrentState = State.GettingImage;
                        }
                        else
                        {
                            SearchText = paramVal?.TextOrSpriteValue ?? "";
                            SelectedValue = null;
                        }
                    }
                    paramEditor = AnyVarEditor;
                    break;


            }
            SearchPropName = GetAssetName(item);
            m_searchResult.Value = new string[0];
            RestartFilterCoroutine(paramEditor);
            m_currentEditingParam = key;
        }
        private void GetBackToList(T item, IWTSParameterEditor<T> parameterEditor)
        {
            if ((CurrentState == State.GettingText || (CurrentState == State.GettingAny && IsTextVariable)) && IsVariable)
            {
                var cl = CommandLevel.OnFilterParamByText(GetCurrentParamString(), out _);
                if (m_searchResult.Value.Contains(SearchText))
                {
                    SelectedValue = CommandLevel.FromParameterPath(CommandLevel.GetParameterPath(SelectedValue ?? "").Take(cl.level).Concat(new[] { SearchText }));
                }
                if (parameterEditor.HoverIdx > 0 && parameterEditor.HoverIdx < m_searchResult.Value.Length)
                {
                    SelectedValue = CommandLevel.FromParameterPath(CommandLevel.GetParameterPath(SelectedValue ?? "").Take(cl.level).Concat(new[] { m_searchResult.Value[parameterEditor.HoverIdx] }));
                }
                SetTextParameter(item, m_currentEditingParam, GetCurrentParamString());
            }
            else
            {
                SetTextParameter(item, m_currentEditingParam, CurrentState == State.GettingText && SearchText == "" ? null : GetCurrentParamString());
            }
            CurrentState = State.List;
        }

        internal void RestartFilterCoroutine(IWTSParameterEditor<T> paramEditor, string autoselect = null)
        {
            if (m_searchCoroutine != null)
            {
                ModInstance.Controller.StopCoroutine(m_searchCoroutine);
            }
            m_searchCoroutine = ModInstance.Controller.StartCoroutine(OnFilterParam(paramEditor, autoselect));
        }

        private IEnumerator OnFilterParam(IWTSParameterEditor<T> paramEditor, string autoselect)
        {
            yield return 0;
            var baseArr = GetCurrentParamString().Count(x => x == '/') >= 3 ? new[] { "<color=#FFFF00><<</color>" } : new string[0];
            yield return m_searchResult.Value = baseArr.AddRangeToArray(paramEditor.OnFilterParam(this)?.Select(x => x.IsNullOrWhiteSpace() ? GUIKwyttoCommons.v_empty : x).ToArray() ?? new string[0]);
            if (autoselect != null)
            {
                var autoSelectVal = Array.IndexOf(m_searchResult.Value, autoselect);
                if (autoSelectVal > 0)
                {
                    paramEditor.OnHoverVar(this, autoSelectVal, CommandLevel.OnFilterParamByText(GetCurrentParamString(), out _));
                }
            }
        }
        #endregion

        #region Param editor fields
        internal readonly Wrapper<string[]> m_searchResult = new Wrapper<string[]>();
        private Coroutine m_searchCoroutine;
        public Dictionary<string, WEImageInfo> currentFolderAtlas;

        public bool IsLocal { get; private set; } = false;
        public bool IsTextVariable { get; set; } = false;
        public string SearchText { get; set; }
        public string SelectedFolder { get; private set; }
        public string SearchPropName { get; private set; }
        public string SelectedValue { get; private set; }
        public string VariableDescription { get; private set; }
        public bool IsVariable { get; private set; } = false;
        internal State CurrentState { get; private set; } = State.List;

        public void SetLocal(bool newVal) => IsLocal = newVal;
        public void ClearSelectedFolder() => SelectedFolder = null;
        public void SetSelectedFolder(string val) => SelectedFolder = val ?? "";
        public void ClearSelectedValue() => SelectedValue = null;
        public void SetSelectedValue(string val) => SelectedValue = val ?? "";
        public void SetVariableDescription(string val) => VariableDescription = val;
        public void SetIsVariable(bool val) => IsVariable = val;
        #endregion

        #region Abstract methods
        protected abstract string GetAssetName(T item);
        protected abstract void SetTextParameter(T item, int currentEditingParam, string paramValue);
        protected abstract void DrawListing(Vector2 areaRect, T item, bool isEditable);
        #endregion

    }
}
