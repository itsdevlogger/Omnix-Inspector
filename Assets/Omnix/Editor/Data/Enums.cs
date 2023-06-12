namespace Omnix.Data
{
    public enum ElementType
    {
        // Layouts
        VerticalLayout,
        HorizontalLayout,
        PagedLayout,

        // Elements
        InputField,
        Button,
        Label,
        HelpBox,
        Spacer,
    }

    public enum SizeType
    {
        Auto,
        Pixels
    }

    public enum ShowIfTargetType
    {
        AlwaysShow,
        AlwaysHide,
        Method,
        Property,
        Field
    }

    public enum BaseStyles
    {
        Auto,
        MiniLabel,
        LargeLabel,
        BoldLabel,
        MiniBoldLabel,
        CenteredGreyMiniLabel,
        WordWrappedMiniLabel,
        WordWrappedLabel,
        LinkLabel,
        WhiteLabel,
        WhiteMiniLabel,
        WhiteLargeLabel,
        WhiteBoldLabel,
        RadioButton,
        MiniButton,
        MiniButtonLeft,
        MiniButtonMid,
        MiniButtonRight,
        MiniPullDown,
        TextField,
        TextArea,
        MiniTextField,
        NumberField,
        Popup,
        ObjectField,
        ObjectFieldThumb,
        ObjectFieldMiniThumb,
        ColorField,
        LayerMaskField,
        Toggle,
        Foldout,
        FoldoutPreDrop,
        FoldoutHeader,
        FoldoutHeaderIcon,
        ToggleGroup,
        Toolbar,
        ToolbarButton,
        ToolbarPopup,
        ToolbarDropDown,
        ToolbarTextField,
        InspectorDefaultMargins,
        InspectorFullWidthMargins,
        HelpBox,
        ToolbarSearchField,
        IconButton,
        SelectionRect
    }
}