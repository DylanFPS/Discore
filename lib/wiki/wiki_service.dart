import 'dart:async';

import 'package:angular2/angular2.dart';

import 'json_page_loader.dart';
import 'wiki_page.dart';

@Injectable()
class WikiService {
  Map<String, WikiPage> _cachedPages;

  Future<Map<String, WikiPage>> getPages() async {
    if (_cachedPages == null) {
      _cachedPages = await JsonPageLoader.loadJson();
    } else {
      print('from cache');
    }

    return _cachedPages;
  }
}