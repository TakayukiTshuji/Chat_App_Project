# ChatAppProject
## ChatCtl
APIの説明：
メッセージ取得、投稿、編集、削除を行います。
### メソッド：GET
説明：
チャット部屋のメッセージを取得します。
URLパラメータとクッキーの`session_id`が必要です。
ユーザが登録されたチャット部屋と一般公開のチャット部屋以外は取得できません。

URL：`/api/ChatCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| room | int | 部屋ID |

結果：
- 成功時（部屋ID 1の場合の例）
```
{
  "result": [
    {
      "message": "テスト",
      "user": "test",
      "date": "2023-10-03T01:00:50.621Z",
      "hidden": false,
      "message_id": 0,
      "room": 1,
      "edited": false,
      "nickname_jp": "admin",
      "nickname_en": "admin"
    }
  ],
  "status": true,
  "message": "成功"
}
```
- 失敗時（存在しない部屋IDを指定した場合の例）
```
{
  "result": null,
  "status": false,
  "message": "部屋が存在しません"
}
```
### メソッド：POST
説明：
メッセージを指定した部屋IDのチャット部屋に投稿します。
URLパラメータとクッキーの`session_id`が必要です。
ユーザが登録されたチャット部屋と一般公開のチャット部屋以外には投稿できません。

URL：`/api/ChatCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| message | string | 本文 |
| room | int | 部屋ID |

結果：
- 成功時
```
{
  "result": {
    "message": "asdasdasd",
    "user": "admin",
    "date": "2023-10-06T09:37:01.7880818+09:00",
    "hidden": false,
    "message_id": 2,
    "room": 0,
    "edited": false,
    "nickname_jp": null,
    "nickname_en": null
  },
  "status": true,
  "message": "成功"
}
```
- 失敗時
```
{
  "result": null,
  "status": false,
  "message": "部屋が存在しません"
}
```

### メソッド：DELETE
説明：
指定された部屋のメッセージを削除します。
メッセージ投稿者かモデレータのみが削除できます。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| room | int | 部屋ID |
| messageId | int | メッセージID |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功"
}
```
- 失敗時（既に削除されたメッセージを削除しようとした場合の例）
```
{
  "status": false,
  "message": "すでに非表示です"
}
```
### メソッド：PATCH
説明：
メッセージを編集します。
投稿者のみが編集できます。
編集するとメッセージのisEditedがtrueになり、編集済みとなります。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| room | int | チャット部屋ID |
| messageId | int | メッセージID |
| message | string | 編集後のメッセージ |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功"
}
```
- 失敗時（存在しないメッセージを編集しようとした場合の例）
```
{
  "status": false,
  "message": "存在しないメッセージ"
}
```

## ChatReadCountCtl
説明：既読状態にする、既読状態の取得を行います。
### メソッド：GET
説明：
指定されたチャット部屋のメッセージの既読状態を取得します。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatReadCountCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| roomId | int | 部屋ID |

結果：
- 成功時
```
{
  "message": "成功",
  "status": true,
  "result": {
    "roomId": 1,
    "counts": {
      "0": [
        "admin"
      ],
      "1": [
        "admin"
      ]
    }
  }
}
```
countsにメッセージIDごとに既読を付けたユーザIDのリストが格納されています。

- 失敗時（指定された部屋IDに一つも既読がついていない場合）
```
{
  "message": "既読が一つもありません",
  "status": false,
  "result": null
}
```

### メソッド：POST
説明：
メッセージを既読状態にします。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatReadCountCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| roomId | int | 部屋ID |
| messageId | int | メッセージID |

結果：
- 成功時
```
{
  "message": "成功",
  "status": true
}
```
- 失敗時（ユーザ認証に失敗した場合など）
```
{
  "message": "[理由]",
  "status": false
}
```

## ChatRoomCtl
APIの説明：
チャット部屋一覧の取得、部屋の作成、削除、公開設定と部屋名の変更を行います。

### メソッド：GET
説明：
チャット部屋の一覧を取得します。
自分が登録されていないプライベート部屋は取得できません。
クッキーの`session_id`が必要です。

URL：`/api/ChatRoomCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| なし | なし | パラメータは必要ありません |

結果：
- 成功時
```
{
  "result": [
    {
      "name": "General",
      "room_id": 0,
      "hidden": false,
      "creator": "admin",
      "tags": [
        "public"
      ],
      "isPrivate": false,
      "whiteList": []
    },
    {
      "name": "プロ研",
      "room_id": 1,
      "hidden": false,
      "creator": "admin",
      "tags": [
        "private"
      ],
      "isPrivate": true,
      "whiteList": [
        "admin",
        "ruchi",
        "nekoyashiki",
        "foo"
      ]
    }
  ],
  "status": true,
  "message": "成功"
}
```
- 失敗時（クッキーのsession_idが存在していない場合の例）
```
{
  "result": null,
  "status": false,
  "message": "クッキーが存在しません"
}
```

### メソッド：POST
説明：
チャット部屋を作成します。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatRoomCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| name | string | チャット部屋の名前 |
| isPrivate | bool | 閲覧できるユーザを制限したい場合にtrueにする |
| isDm | bool | ダイレクトメッセージの場合にtrue |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功"
}
```
- 失敗時（既に存在している部屋名）
```
{
  "status": false,
  "message": "既存の部屋名"
}
```

### メソッド：DELETE
説明：
部屋を削除します。
サーバ管理者かモデレータのみが部屋を削除できます。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatRoomCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| room | int | 部屋ID |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功"
}
```
- 失敗時（存在しないか既に削除された部屋を削除しようとした場合の例）
```
{
  "status": false,
  "message": "部屋が存在しません"
}
```

### メソッド：PATCH
説明：
チャット部屋の名前、公開設定、ホワイトリストを編集できます。
部屋の作成者のみが編集できます。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatRoomCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| roomId | int | 変更対象の部屋ID |
| roomName | string | 変更後の部屋名 |
| isPrivate | bool | プライベートかどうか |
| Request body | Content-Type: application/json | ホワイトリストのユーザIDの配列をJSONで送る |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功",
  "result": {
    "name": "General YOO",
    "room_id": 0,
    "hidden": false,
    "creator": "admin",
    "tags": [
      "public"
    ],
    "isPrivate": false,
    "whiteList": [
      "string",
      "admin"
    ]
  }
}
```
- 失敗時（存在しない部屋を編集しようとした場合の例）
```
{
  "status": false,
  "message": "部屋が存在しません",
  "result": null
}
```

## ChatSessionCtl
APIの説明：ユーザ認証のAPIです。
セッションIDをクッキーに保存して管理しています。
セッションの有効期限は一週間です。

### メソッド：GET
説明：
現在のセッションのユーザの情報とセッションの情報を取得します。
特に用途なし。
クッキーの`session_id`が必要です。

URL：`/api/ChatSessionCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| なし | なし | パラメータは必要ありません |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功",
  "result": {
    "sessionId": "d87f004f-cdfb-4a95-a746-21c5cfaf9bbe",
    "userId": "admin",
    "expirationDate": "2023-10-10T09:33:21.25+09:00"
  },
  "user": {
    "userId": "admin",
    "familyName": "admin",
    "firstName": "admin",
    "studentId": "admin",
    "language": "日本語",
    "nickname_ja": "admin",
    "nickname_en": "admin",
    "tags": [
      "admin",
      "moderator"
    ]
  }
}
```
- 失敗時（クッキーが存在しない場合の例）
```
{
  "status": false,
  "message": "クッキーが存在しません",
  "result": null,
  "user": null
}
```

### メソッド：POST
説明：
ログイン処理です。
ログイン成功時にクッキーのsession_idにセッションIDが追加されます。
URLパラメータが必要です。

URL：`/api/ChatSessionCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| userId | string | ユーザID |
| password | string | ***パスワードの ハッシュ値 （SHA256）大文字*** |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功",
  "result": {
    "sessionId": "5dbc75d1-6ec1-4c08-81c5-15b4546a0fc9",
    "userId": "cat1",
    "expirationDate": "2023-10-13T11:22:46.6321628"
  }
}
```
- 失敗時（存在しないユーザの場合の例）
```
{
  "status": false,
  "message": "存在しないユーザーです。",
  "result": null
}
```

### メソッド：PATCH
説明：
セッションの有効期限を一週間延長します。
すでに期限切れのセッションは延長できません。
クッキーの`session_id`が必要です。

URL：`/api/ChatSessionCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| なし | なし | パラメータは必要ありません |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功",
  "result": {
    "sessionId": "d87f004f-cdfb-4a95-a746-21c5cfaf9bbe",
    "userId": "admin",
    "expirationDate": "2023-10-10T09:33:21.25+09:00"
  }
}
```
- 失敗時（クッキーが存在しない場合の例）
```
{
  "status": false,
  "message": "クッキーが存在しません",
  "result": null
}
```

### メソッド：DELETE
説明：
ログオフです。
セッションとクッキーを削除します。
クッキーの`session_id`が必要です。

URL：`/api/ChatSessionCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| なし | なし | パラメータは必要ありません |


結果：
- 成功時
```
{
  "status": true,
  "message": "成功",
  "result": {
    "sessionId": "5dbc75d1-6ec1-4c08-81c5-15b4546a0fc9",
    "userId": "cat1",
    "expirationDate": "2023-10-13T11:22:46.632+09:00"
  }
}
```
- 失敗時（クッキーが存在しない場合の例）
```
{
  "status": false,
  "message": "クッキーが存在しません",
  "result": null
}
```

## ChatUserCtl
APIの説明：ユーザの登録、削除、編集などを行います。

### メソッド：GET
説明：
ユーザIDとパスワードのハッシュ値からユーザの情報を取得します。
特に用途なし。
URLパラメータが必要です。

URL：`/api/ChatUserCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| userId | string | ユーザID |
| password | string | パスワードのハッシュ値（SHA256）大文字 |

結果：
- 成功時
```
{
  "result": {
    "userId": "cat1",
    "familyName": "cat",
    "firstName": "cat",
    "studentId": "GP23A999",
    "language": "日本語",
    "nickname_ja": "ねこ",
    "nickname_en": "cat",
    "tags": []
  },
  "status": true,
  "message": "成功"
}
```
- 失敗時（パスワードが違う場合の例）
```
{
  "result": null,
  "status": false,
  "message": "パスワードが違います。"
}
```

### メソッド：POST
説明：
ユーザ登録です。
ユーザIDは変更できません。
URLパラメータが必要です。

URL：`/api/ChatUserCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| userId | string | ユーザID　４文字以上の半角英数字 |
| familyName | string | 苗字 |
| firstName | string | 名前 |
| language | string | （言葉の）言語 |
| studentId | string | 生徒番号（GP00A000など） |
| password | string | パスワード（生の値） |
| nickname_ja | string | ニックネーム日本語 |
| nickname_en | string | ニックネーム英語 |

結果：
- 成功時
```
{
  "result": {
    "userId": "return",
    "familyName": "return",
    "firstName": "taro",
    "studentId": "gp99a999",
    "language": "日本語",
    "nickname_ja": "戻り値",
    "nickname_en": "return value",
    "tags": []
  },
  "status": true,
  "message": "成功"
}
```
- 失敗時（既存のユーザIDを登録しようとした場合）
```
{
  "result": null,
  "status": false,
  "message": "既存のユーザーID"
}
```

### メソッド：DELETE
説明：
ユーザを削除します。
URLパラメータが必要です。


URL：`/api/ChatUserCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| userId | string | ユーザID |
| password | string | パスワードのハッシュ値（sha256）大文字 |

結果：
- 成功時
```
{
  "result": {
    "userId": "return",
    "familyName": "return",
    "firstName": "taro",
    "studentId": "gp99a999",
    "language": "日本語",
    "nickname_ja": "戻り値",
    "nickname_en": "return value",
    "tags": []
  },
  "status": true,
  "message": "成功"
}
```
- 失敗時
```
{
  "result": null,
  "status": false,
  "message": "存在しないユーザーです。"
}
```

### メソッド：PATCH
説明：
ユーザの情報を編集します。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatUserCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| userId | string | ユーザID　４文字以上の半角英数字 |
| password | string | 現在のパスワード（生の値） |
| familyName | string | 苗字 |
| firstName | string | 名前 |
| language | string | （言葉の）言語 |
| newPassword | string | 新しいパスワード（生の値） |
| studentId | string | 生徒番号（GP00A000など） |

| nickname_ja | string | ニックネーム日本語 |
| nickname_en | string | ニックネーム英語 |

結果：
- 成功時
```
{
  "result": {
    "userId": "hogehoge",
    "familyName": "[編集済み]",
    "firstName": "[編集済み]",
    "studentId": "gp23hogehoge",
    "language": "[編集済み]",
    "nickname_ja": "[編集済み]",
    "nickname_en": "[編集済み]",
    "tags": [
      "moderator"
    ]
  },
  "status": true,
  "message": "成功"
}
```
- 失敗時（現在のパスワードが違う場合の例）
```
{
  "result": null,
  "status": false,
  "message": "パスワードが違います。"
}
Response
```

## ChatWritingCtl
APIの説明：書き込み中の状態に関するAPIです。


### メソッド：GET
説明：
チャット部屋ごとに現在書き込んでいるユーザのIDのリストを返します。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatWritingCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| roomId | int | 部屋ID |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功",
  "result": [
    "admin"
  ]
}
```
- 失敗時（誰も書き込んでいない場合の例）
```
{
  "status": false,
  "message": "誰も書き込んでいません",
  "result": []
}
```

### メソッド：POST
説明：
指定された部屋でユーザを書き込み中状態にします。
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatWritingCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| roomId | int | 部屋ID |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功"
}
```
- 失敗時（認証失敗の例）
```

```

### メソッド：DELETE
説明：
URLパラメータとクッキーの`session_id`が必要です。

URL：`/api/ChatWritingCtl`

| パラメータ名 | 型 | 説明 |
| -- | -- | -- |
| roomId | int | 部屋ID |

結果：
- 成功時
```
{
  "status": true,
  "message": "成功"
}
```
- 失敗時（認証失敗時の例）
```
{
  "result": null,
  "status": false,
  "message": "クッキーが存在しません"
}
```
