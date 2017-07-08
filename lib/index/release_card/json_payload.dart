import 'package:markdown/markdown.dart';
import 'package:angular2/security.dart';

class JsonPayload {
  DomSanitizationService _dSS;

  JsonPayload(this._dSS);

  Stopwatch stopWatch;
  dynamic data;

  /// Gets the markdown rendered body of the release
  SafeHtml getBody()
    => _dSS.bypassSecurityTrustHtml(markdownToHtml(data['body'], extensionSet: ExtensionSet.gitHub));

  /// Gets the release's tag name
  String getTagName()
    => data['tag_name'] as String;

  /// Gets the release's published date
  DateTime getPublishedDate()
    => DateTime.parse(data['published_at']);

  /// Gets the total time taken, in milliseconds, for the initial http call
  String getMillisecondsTaken()
    => stopWatch.elapsedMilliseconds.toString();
}