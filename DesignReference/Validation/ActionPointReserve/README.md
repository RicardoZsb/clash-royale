# 行动点储备池

设计来源：[Figma 战斗主界面](https://www.figma.com/design/Nbp7FeVnTaPPgSxHbdgL8e?node-id=1-3)。读取于 2026-11-01。

- 入口底板：71:212；行动点图标：70:188；数值：71:208；三角：214:118。
- Figma 当前有入口及三角，没有储备池展开面板。展开面板复用项目现有任务下拉菜单的背景、描边、圆角及字体。
- 原始三角 SVG 保存为 FigmaArrow.svg，转换后的本地贴图为 Assets/ClashRoyaleUI/Art/ReserveDropdownArrow.png。
- 行动点图标 FigmaActionPoint.png 与现有 Art/db365.png 的 SHA256 相同，复用现有素材。

组件：Assets/ClashRoyaleUI/Prefabs/Components/ActionPointReserveDropdown.prefab，嵌套在 BattleHome.prefab 内。所有控件和按钮事件均序列化保存，可在 Inspector 调整。author_reserve.cs 是本次离线编辑脚本，场景运行不会调用。

交互代码：Assets/ClashRoyaleUI/Runtime/UIActionPointReserve.cs。初始行动点 16/24，储备池 16。提取采用补满上限的演示规则：本次提取 8 点，行动点变成 24/24，储备池保留 8 点；已满时禁用提取。数值是本地演示状态，重新进入 Play 后重置。点击空白或再次点击入口关闭，离开页面也会关闭。
