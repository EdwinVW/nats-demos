sqlcmd -S localhost,1434 -U sa -P 8jkGh47hnDw89Haq8LN2 -i reset-db.sql

Get-ChildItem -Path D:\Temp\NATSDemos\Store -Include * | remove-Item -recurse
