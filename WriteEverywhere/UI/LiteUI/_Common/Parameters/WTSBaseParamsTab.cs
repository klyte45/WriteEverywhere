using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using WriteEverywhere.Xml;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace WriteEverywhere.UI
{
    public abstract class WTSBaseParamsTab<T> : IGUITab<T>
    {
        internal enum State
        {
            List,
            GettingImage,
            GettingFolder,
            GettingText
        }

        public abstract Texture TabIcon { get; }
        private Vector2 m_leftPanelScroll;
        private Vector2 m_rightPanelScroll;
        private int m_currentEditingParam = 0;

        internal readonly string Text = Color.green.ToRGB();
        internal readonly string Image = Color.cyan.ToRGB();
        internal readonly string Folder = Color.yellow.ToRGB();

        internal readonly WTSParameterVariableEditor<T> TextVarEditor = new WTSParameterVariableEditor<T>();
        internal readonly WTSParameterImageEditor<T> ImageVarEditor = new WTSParameterImageEditor<T>();
        internal readonly WTSParameterFolderEditor<T> FolderVarEditor = new WTSParameterFolderEditor<T>();

        #region Basic Behavior
        internal bool ShowTabsOnTop() => CurrentState == State.List;

        public void Reset()
        {
            CurrentState = State.List;
            m_currentEditingParam = 0;
        }
        public bool DrawArea(Vector2 areaRect, ref T item, int _)
        {
            switch (CurrentState)
            {
                case State.List:
                    DrawListing(areaRect, item);
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
            }
            return false;
        }

        private void DrawImagePicker(T item, Vector2 areaRect) => DrawSelectorView(item, areaRect, ImageVarEditor);

        private void DrawFolderPicker(T item, Vector2 areaRect) => DrawSelectorView(item, areaRect, FolderVarEditor);

        private void DrawVariablePicker(T item, Vector2 areaRect) => DrawSelectorView(item, areaRect, TextVarEditor, IsVariable);

        #endregion

        #region Param editor commons

        private void OnClearSearch(bool isText)
        {
            SelectedFolder = null;
            SelectedValue = null;
            m_searchResult.Value = new string[0];
            if (isText)
            {
                SearchText = "";
                if (IsVariable)
                {
                    RestartFilterCoroutine();
                }
            }
            else
            {
                RestartFilterCoroutine();
            }
        }
        private void DrawSelectorView(T item, Vector2 areaRect, IWTSParameterEditor<T> paramEditor, bool showRightPanel = true)
        {
            paramEditor.DrawTop(this, areaRect);
            using (new GUILayout.HorizontalScope())
            {
                using (var scroll = new GUILayout.ScrollViewScope(m_leftPanelScroll, false, true, GUILayout.Width(areaRect.x / 2), GUILayout.Height(areaRect.y - 80)))
                {
                    paramEditor.DrawLeftPanel(this, areaRect);
                    m_leftPanelScroll = scroll.scrollPosition;
                };
                using (new GUILayout.VerticalScope(GUILayout.Width(areaRect.x / 2), GUILayout.Height(areaRect.y - 80)))
                {
                    if (showRightPanel)
                    {
                        GUILayout.Label(GetCurrentParamString(), new GUIStyle(GUI.skin.label) { normal = new GUIStyleState() { textColor = Color.yellow } });
                        GUILayout.Space(8);
                        using (var scroll = new GUILayout.ScrollViewScope(m_rightPanelScroll))
                        {
                            paramEditor.DrawRightPanel(this, default);
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
                    OnClearSearch(paramEditor.IsText);
                }
                if (GUILayout.Button("OK"))
                {
                    GetBackToList(item);
                }
            };
        }

        internal string GetCurrentParamString()
        {
            switch (CurrentState)
            {
                case State.GettingImage:
                    return $"{(IsLocal ? "image" : "assetImage")}://{(SelectedFolder is null ? "" : (SelectedFolder == "" ? "<ROOT>" : SelectedFolder) + "/")}{SelectedValue}";
                case State.GettingFolder:
                    return $"{(IsLocal ? "folder" : "assetFolder")}://{(SelectedFolder == "" ? "<ROOT>" : SelectedFolder)}";
                case State.GettingText:
                    return IsVariable ? $"var://{SelectedValue}" : SearchText;
            }
            return null;
        }
        protected void GoToPicker(int key, TextContent targetContentType, TextParameterWrapper paramVal, T item)
        {
            switch (targetContentType)
            {
                case TextContent.ParameterizedSpriteSingle:
                    SearchText = "";
                    IsLocal = paramVal.IsLocal;
                    SelectedFolder = paramVal.AtlasName.TrimToNull();
                    SelectedValue = paramVal.TextOrSpriteValue;
                    CurrentState = State.GettingImage;
                    break;
                case TextContent.ParameterizedSpriteFolder:
                    SearchText = "";
                    IsLocal = paramVal.IsLocal;
                    SelectedFolder = paramVal.AtlasName.TrimToNull();
                    CurrentState = State.GettingFolder;
                    break;
                case TextContent.ParameterizedText:
                case TextContent.TextParameterSequence:
                    IsVariable = paramVal.ParamType == TextParameterWrapper.ParameterType.VARIABLE;
                    CurrentState = State.GettingText;
                    if (IsVariable)
                    {
                        SearchText = "";
                        SelectedValue = paramVal.GetOriginalVariableParam();
                    }
                    else
                    {
                        SearchText = paramVal.TextOrSpriteValue ?? "";
                        SelectedValue = null;
                    }
                    break;
            }
            SearchPropName = GetAssetName(item);
            m_searchResult.Value = new string[0];
            RestartFilterCoroutine();
            m_currentEditingParam = key;
        }
        private void GetBackToList(T item)
        {
            if (CurrentState == State.GettingText && IsVariable)
            {
                var cl = CommandLevel.OnFilterParamByText(GetCurrentParamString(), out _);
                if (m_searchResult.Value.Contains(SearchText))
                {
                    SelectedValue = CommandLevel.FromParameterPath(CommandLevel.GetParameterPath(SelectedValue ?? "").Take(cl.level).Concat(new[] { SearchText }));
                }
                if (TextVarEditor.HoverIdx > 0 && TextVarEditor.HoverIdx < m_searchResult.Value.Length)
                {
                    SelectedValue = CommandLevel.FromParameterPath(CommandLevel.GetParameterPath(SelectedValue ?? "").Take(cl.level).Concat(new[] { m_searchResult.Value[TextVarEditor.HoverIdx] }));
                }
                SetTextParameter(item, m_currentEditingParam, GetCurrentParamString());
            }
            else
            {
                SetTextParameter(item, m_currentEditingParam, CurrentState == State.GettingText && SearchText == "" ? null : GetCurrentParamString());
            }
            CurrentState = State.List;
        }

        internal void RestartFilterCoroutine(string autoselect = null)
        {
            if (m_searchCoroutine != null)
            {
                ModInstance.Controller.StopCoroutine(m_searchCoroutine);
            }
            m_searchCoroutine = ModInstance.Controller.StartCoroutine(OnFilterParam(autoselect));
        }

        private IEnumerator OnFilterParam(string autoselect)
        {
            yield return 0;
            if (CurrentState == State.GettingImage || CurrentState == State.GettingFolder)
            {
                yield return m_searchResult.Value = ImageVarEditor.OnFilterParam(this);
            }
            else if (CurrentState == State.GettingText)
            {
                yield return m_searchResult.Value = new[] { "<color=#FFFF00><<</color>" }.Concat(TextVarEditor.OnFilterParam(this)?.Select(x => x.IsNullOrWhiteSpace() ? GUIKlyteCommons.v_empty : x) ?? new string[0]).ToArray();
                if (autoselect != null)
                {
                    var autoSelectVal = Array.IndexOf(m_searchResult.Value, autoselect);
                    if (autoSelectVal > 0)
                    {
                        TextVarEditor.OnHoverVar(this, autoSelectVal, CommandLevel.OnFilterParamByText(GetCurrentParamString(), out _));
                    }
                }
            }
        }
        #endregion

        #region Param editor fields
        internal readonly Wrapper<string[]> m_searchResult = new Wrapper<string[]>();
        private Coroutine m_searchCoroutine;
        public UITextureAtlas currentFolderAtlas;

        public bool IsLocal { get; private set; } = false;
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
        protected abstract void DrawListing(Vector2 areaRect, T item);
        #endregion

    }
}
