class JsonPayload {
  dynamic data;

  JsonPayload(this.data);

  String getTagName()
    => data['tag_name'] as String;

  String getZipURL()
    => data['zipball_url'] as String;
}