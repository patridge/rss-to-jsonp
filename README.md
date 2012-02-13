System for proxying an RSS XML feed to JSON/JSONP.

TODO:

Fix serialization to JSON when the first character of a string is already encoded (e.g., "<" => "\u003c")

    "content": "\u003cdiv..."