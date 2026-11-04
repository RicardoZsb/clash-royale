from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
root=Path(__file__).resolve().parents[1]/'Assets'/'ClashRoyaleUI'/'Fonts'
source=Path('C:/Windows/Fonts/NotoSansSC-VF.ttf')
for name,weight in [('NotoSansSC',400),('NotoSansSC-Bold',700)]:
    font=TTFont(source)
    instantiateVariableFont(font,{'wght':weight},inplace=True)
    font.save(root/(name+'.ttf'))
font=TTFont(root/'Inter.ttf')
if 'fvar' in font:
    axes={a.axisTag:(400 if a.axisTag=='wght' else 14 if a.axisTag=='opsz' else a.defaultValue) for a in font['fvar'].axes}
    instantiateVariableFont(font,axes,inplace=True)
    font.save(root/'Inter.ttf')
print('Prepared regular and bold source fonts.')
