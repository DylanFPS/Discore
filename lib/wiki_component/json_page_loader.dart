import 'dart:convert';
import 'dart:html';
import 'dart:async';
import 'wiki_page.dart';

class JsonPageLoader {
  static Future<Map<String, WikiPage>> loadJson() async {
    Map <String, WikiPage> toReturn = new Map();
    await HttpRequest.getString('payload.json')
      .then((String d) {
        var data = JSON.decode(d) as Map<String, String>;
        data.forEach((key, value) {
          toReturn.putIfAbsent(key, () => new WikiPage(value));
        });
      });
    return toReturn;
  }
}