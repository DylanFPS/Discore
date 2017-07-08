import 'dart:convert';
import 'dart:html';
import 'dart:async';

import 'wiki_page.dart';

class JsonPageLoader {
  static Future<Map<String, WikiPage>> loadJson() async {
    Map <String, WikiPage> toReturn = new Map();

    String payload = await HttpRequest.getString('payload.json');
    var data = JSON.decode(payload) as Map<String, String>;
        
    data.forEach((key, value) {
      toReturn.putIfAbsent(key, () => new WikiPage(value));
    });

    return toReturn;
  }
}