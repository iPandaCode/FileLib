   M S B A M L   `   `   `  ����L  HFtpUserControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:FtpUserControlLibFtpUserControlLib  5      4    .FtpUserControlLib.FtpDownUploadFileUserControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      1localclr-namespace:FtpUserControlLib     5        � 	Resources  5	         EillseButton �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius �� Triggers P� Property P� Value3  -System.Windows.Media.Effects.DropShadowEffect	   ShadowDepth
   Color   Opacity   
BlurRadius��%�  &	        �� 5
   
   " ��6%   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      �� $ 14��6!   #��z��63   #��z��6S   #��z��6u   ._�5      �� #��z��6(   $��Center=�6I   $��Center=�6d   #��z��6�   6�   5      5      5      �� 5       � ��
 ���� 6A   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � ��$ Resources/#iconfont)�6+   6I    5      P� 5       �@J�IsMouseOver . . �5      ��  �@J�Effect 5        5      $	 0Q�6/   $
 White��6?   $ 1Q�6M   $ 6Q�6Y   5      �� 5       � �� �	#FF89BAF56G   5      5!      5"      .��5#      �  � ColumnDefinitions 5$   
   �� 5%      $��1* �6   6,   �� 5&      $��1* �6   6,   �� 5'      $	��280 �6   6-   �� 5(      $��1* �6   6,   �� 5)      $��1* �6   6,   .+�5+   
   � -   5         �Name$
 pBar��5+   :   $��0��6   $��5��6&   
������ $��0q�6a   ���	#FFCFF3EB6�   �� 5,   
   $��0��6    �� Orientation$ Vertical=�6%   $��Center=�6<   .+�5-      �� -   5         ��Name$ 	tbkMethod��5-   #   $��3q�6   $	����66   $��18(�6F   $��Resources/#iconfont)�6T   ���	#FF9C9C9C$��Center=�6�    �� TextAlignment$ Center=�6�   6�   5.      �� 5/   
   $��1��6   $ Vertical=�6%   $��Center=�6<   .+�50      �� -   5        $ tbkExtension��50   #   $��3q�6   $	����69   $��30(�6I   $��Resources/#iconfont)�6W   $��Center=�6y   $ Center=�6�   ���	#FFB9C3D36�   51      � 52   
   $��2��6    � RowDefinitions 53      �� 54      $��5* �6    6.   �� 55      $��4* �6    6.   .+�57      �� -   5        $ tbkFileName��57   7   $��0��6   $��10,5,0,0q�6%   $��	文件名��6L   $��16(�6W   $��Bottom=�6e   6�   � 58      $��1��6   $��10,0,0,0q�6&    � Orientation$ 
Horizontal=�68   $
��Left=�6Q   $	��Top=�6l   .+�59      � -   5         �Name$ wpUnnormalTip��59      $y�	Collapsed=�63   .+�5:      �� -   5        $ tbkUnnormalTip��5:   1   $��0,6,6,6q�6    $
��auto��6I   $��任务异常��6V   $��10(�6b   ���	#FFFB9191$��Stretch=�6�   $ Center=�6�   6�   5;      � 5<      -   5        $ wpNormalTip��5<      .+�5=      �� -   5        $ tbkFileSize��5=   1   $��0,6,6,6q�6    $
��auto��6F   $��666.66MB��6S   $��10(�6c   ���	#FF9C9C9C$��Stretch=�6�   $ Center=�6�   6�   �� 5>      -	   5        $ tbkNormalTip��5>   +   $��6q�6    $
��auto��6A   $��排队等待中��6N   $��10(�6[   ���	#FF9C9C9C$��Stretch=�6�   $ Center=�6�   6�   5?      5@      5A      �� 5B   
   $��3��6   $ Vertical=�6%   $��Center=�6<   .+�5C      �� -
   5         ��Name$ btnRun��5C       $��3q�6   $��25��60   $��25��6;   $	����6G   $��16(�6Z   #��[  6h   5D      5E      �� 5F   
   $��4��6   $ Vertical=�6%   $��Center=�6<   .+�5G      �� -   5        $ 	btnCancel��5G       $��3q�6   $��25��63   $��25��6>   $	����6J   $��16(�6]   #��[  6k   5H      5I      5J      5K      