   M S B A M L   `   `   `  ����   PublishResults    PublishResults.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$(  "Mainframe Automation Project (MAP)��$��550.522��$��1055.97�� � 
ResizeMode$ NoResize=� � Icon$ icon.ico��.��� -    �Name$ MainGrid��{�.  $��0,0,-6,0q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $	��197 ��� $	��126 ��� $
��Auto �.+�� -   $ OptionsGrid��$��2��$��3��$	��385��$	��Top=�$��	0,10,10,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $	��130 ��� $
��Auto �.+��� $��1��$��Database��$
��Left=�$��20,5,0,0q�$	��Top=�$��16(�$��31��$��76���� $��2��$��Product��$
��Left=�$��20,5,0,0q�$	��Top=�$��16(�$��31��$��65���� $��4��$��
OS Version��$
��Left=�$��20,5,0,0q�$	��Top=�$��16(�$��31��$��88���� $��5��$
��LPAR��$
��Left=�$��20,5,0,0q�$	��Top=�$��16(�$��31��$��46���� $��6��$��	SubSystem��$
��Left=�$��20,5,0,0q�$	��Top=�$��16(�$��31��$��88���� $��7��$��
Build Info��$
��Left=�$��20,5,0,0q�$	��Top=�$��16(�$��31��$��77���� $��9��$��
Build Name��$
��Left=�$��20,11,0,-18q�$��16(�$	��101���� $��3��$
��Site��$
��Left=�$��20,5,0,0q�$	��Top=�$��16(�$��31��$��36���� -    ��Name$ 
ProductBox��$��2��$��1��$
��Left=�$��0,10,0,0q�$	��Top=�$	��210��$��22���� -   $ OsVersionBox��$��1��$
��Left=�$��0,10,0,0q�$	��Top=�$	��120��$��4��$��22���� -   $ LparBox��$��1��$
��Left=�$��0,10,0,0q�$	��Top=�$	��120��$��5��$��22���� -   $ SubsystemBox��$��1��$
��Left=�$��0,10,0,0q�$	��Top=�$	��210��$��6��$��22���� -   $ SiteBox��$��1��$
��Left=�$��0,10,0,0q�$	��Top=�$	��120��$��3��$��22���� -	    ��Name$ 
PathButton��$��Set Path��$��Right=�$��	0,11,10,0q�$	��Top=�$��92��$��23���� -
    ��Name$ Location��$��suitelog.txt��$��1��$
��Left=�$��10,10,-185,0q�$	��Top=�$	��489��$��31��	� -   	 	�Name$	 DB_TEST��$
��TEST��$��1��$
��Left=�$��	10,14,0,0q�$	��Top=�
 J� RenderTransformOrigin$
 0.049,-0.829&� d� 	IsChecked$ FalseY�$��1��$��16��$��60��	� -   $	 DB_PROD��$
��PROD��$��1��$
��Left=�$��
176,14,0,6q�$��1��$��55���� -    ��Name$ 	BuildInfo��$��1��$
��Left=�$��26��$��0,10,0,0q�$��7�� �� TextWrapping$
 Wrap=�$	��Top=�$	��253���� -   $ LogMessages��$��1��$��1��$
��Left=�$	��264��$��263,41,-591,0q�$
 Wrap=�$�� ��$	��Top=�$	��642��$��8��" �� VerticalScrollBarVisibility$ Visible=�$ �� HorizontalScrollBarVisibility$
 Auto=��� -   $ 	BuildName��$
��Left=�$��26��$��	0,11,0,-3q�$��8��$
 Wrap=�$�� ��$	��Top=�$	��253��$��1��o�  o� 
IsMainMenu . .@�m� $��_Help��.@�m� -   $��_About���� $��0��$��1��$��MAP Results Publisher��$	��Top=�$��20(�$��37��$��25,25,391,0q��� -   $ Submit��$��4��$��Submit��$
��Left=�$��
310,70,0,0q�$	��Top=�$��74��$��22��� -    �Name$ MainProgress��$
��Left=�$��22��$��	25,70,0,0q�$��4��$	��Top=�$	��265��{�.   � IsIndeterminate . �� -   $	 Log��$��View Log��$
��Left=�$��
957,70,0,0q�$��4��$	��Top=�$��75��$��22���� -   $ Cancel��$��Cancel��$
��Left=�$��
404,70,0,0q�$��4��$��74��$��22��$	��Top=�