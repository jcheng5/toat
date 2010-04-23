#!/usr/bin/ruby

# https://secure.maestroweb.com/Login.aspx?OrgID=929
# https://secure.maestroweb.com/Details.aspx?OrgID=929&ItemID=1&selection=36

require 'rubygems'
require 'hpricot'
require 'mechanize'
require 'pp'
require 'highline/import'
#require 'uri'
#require 'net/http'
#require 'net/https'

$agent = Mechanize.new

class NilClass
  def content()
    nil
  end
end

def login(username, password)
  login_page = $agent.get('https://secure.maestroweb.com/Login.aspx?OrgID=929')
  login_form = login_page.form('LoginForm')
  login_form.txtUserField = username
  login_form.txtLogPassword = password
  $agent.submit(login_form, login_form.buttons.first)
end

def get_item(id)
  item = $agent.get("https://secure.maestroweb.com/Details.aspx?OrgID=929&ItemID=#{id}&selection=36")
  return nil unless item.search('#lblItemName').first
  data = {
    :id => id,
    :name => item.search('#lblItemName').first.content,
    :desc => item.search('#lblItemDesc').first.content,
    :value => item.search('#lblItemValue').first.content,
    :donor => item.search('#lblItemDonors').first.content,
    :img => 'https://secure.maestroweb.com/' + item.search('#imgItem').first['src']
  }
end

def download_url(url)
  if url =~ /([^\/]+)(\?|$)/
    filename = $1
    begin
      data = $agent.get_file(url)
    rescue
      return nil
    end
    File.open(filename, 'w') do |f|
      f.write data
    end
  end
end

password = ask("Password: ") {|q| q.echo='*'}
login('Cheng', password)

100.times do |i|
  item = get_item(i)
end