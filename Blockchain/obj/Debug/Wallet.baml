   M S B A M L   `   `   `  ����E  ABlockchain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Blockchain
Blockchain  5   	       Blockchain.Wallet   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:Blockchain     5   	     � Title$  Wallet��5   	   $	��600��6   $	��800��6%   .��5	      � ���	#FF191921 � RowDefinitions 5
   
   �� 5       � ColumnDefinitions 5   
   �� 5      �� 5      $��1 �6   �� 5      .+�5   
   �  5      �� 5      -   5         ��Name$ SendReceiveRow��5       $��50 �68   �� 5      -   5        $ ReceiveDetailsRow��5       $��0 �6;   �� 5      -   5        $ SendDetailsRow��5       $��0 �68   �� 5      -   5        $ 
AddressRow��5       $��80 �64   �� 5      -   5        $ 
BalanceRow��5       $��1* �64   �� 5      -   5        $ 	QrCodeRow��5       $��2* �63   .+�5      �  5      �� 5      �� 5      .+�5       �� $��Receive��6   $��0��6+   ���	#FF37374D$��16(�6R   
���������6s   Z� �� 5!      $
��Send��6   $��1��6(   ���	#FF37374D$��16(�6O   
���������6p   Z� 5"      (� 5#      $��3��6   $
��20,0q�6#   .��5$      ��  �� Orientation$ Vertical=�6   $��0q�64   .+�5%      �� $'��!Your Current SteedyBucks Address:��6   $��Center=�6H   $��Center=�6l   ���#ffffff$��Century Gothic)�6�   �� 5&      $.��(0x984638732902bd32879g7fg43f8b743fr349f2��6   $��Center=�6O   $��Center=�6s   ���#ffffff$��16(�6�   $��Century Gothic)�6�   5'      5(      (� 5)      $��4��6   $
��20,0q�6$   .��5*      �� $��0.14810172 STB��6   $��Center=�61   $��Stretch=�6V   $��Stretch=�6r   ���	#FFE49600$��Century Gothic)�5+   3   ���	#FF37374D$	��0,1q�6g   $��48(�6}   $
��0,30q�6�   5,      �� 5.      -   5         ��Name$ QrImage��5.       $��20q�6   $��5��61   $��Center=�6>   $��Center=�6Y   5/      �� 50   
   $��1��6   ���	#FF37374D6$   � 51   
   $��2��6    52      �� 53      $��50 �6    �� 54      .+�56      �� $��Transaction History��6   $��0��62   $��0,0,0,1q�6?   ���	#FF37374D$
��20,0q�6q   $��Center=�57      $��Center=�66   $��Century Gothic)�6Z   $��16(�6v   
�������6�   58      59      5:      