### Мини документация по API [Электронного Журнала](https://eljur.ru)

Для выполнения всех запросов необходимы cookie файлы.

Возвращает JSON с 20 письмами
```url
https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit=20&offset=0&teacher=21742&status=&companion=&minDate=0
```

Возвращает первые 20 непрочитанных писем
```url
https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit=20&offset=0&teacher=21742&status=unread&companion=&minDate=0
```

Помечает сообщение прочитанным
```url
https://kip.eljur.ru/journal-api-messages-action?method=messages.note_read&idsString=3202690
```

Помечает сообщение непрочитанынм
```url
https://kip.eljur.ru/journal-api-messages-action?method=messages.note_unread&idsString=3202690
```

Возвращает список отправителей
```url
https://kip.eljur.ru/journal-api-messages-action?method=messages.get_senders_list_by_name
```

Возвращает список получателей
```url
https://kip.eljur.ru/journal-api-messages-action?method=messages.get_senders_list_by_name
```

Возвращает категории получателей 
```url
https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipient_structure
```

Возвращает список людей в группе
```url
https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipients_list&key1=school&key2=students&key3=2024%2F2025_1_3%D0%98%D0%A1%D0%98%D0%9F-122%23%23%23%23%230753a2848830c9e5f25229d379c79c7f&dep=null
```
