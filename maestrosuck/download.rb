#!/usr/bin/ruby

# https://secure.maestroweb.com/Login.aspx?OrgID=929
# https://secure.maestroweb.com/Details.aspx?OrgID=929&ItemID=1&selection=36

require 'rubygems'
require 'hpricot'
I_KNOW_I_AM_USING_AN_OLD_AND_BUGGY_VERSION_OF_LIBXML2 = true
require 'mechanize'
require 'pp'
require 'highline/import'
require 'haml'
require 'optparse'

$agent = Mechanize.new

class NilClass
  def content()
    nil
  end
end

class Hash2Object
  def initialize(hash)
    @hash = hash
  end
  def method_missing(symbol, *args)
    return super(symbol, args) if args.length > 0
    @hash[symbol]
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
    :itemid => id,
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


options = {}
optparse = OptionParser.new do |opts|
  opts.banner = "Usage: download.rb -u username -p password"
  
  opts.on('-u', '--username USERNAME', 'Specify the username') do |username|
    options[:username] = username
  end
  opts.on('-p', '--password PASSWORD', 'Specify the password') do |password|
    options[:password] = password
  end
end
optparse.parse!

login(options[:username], options[:password])

items = []
20.times do |i|
  item = get_item(i)
  items << Hash2Object.new(item) if item
end

template = <<EOD
!!! 5
%html
  %head
    %link(rel='stylesheet' type='text/css' href='styles.css')
    %link(rel='stylesheet' type='text/css' href='printstyles.css' media='print')
  %body
    - items.each do |item|
      .item
        %img.picture{:src => item.img}
        .id&= item.itemid
        .name&= item.name
        .desc&= item.desc
        .value&= item.value
        .donor&= item.donor
EOD
haml = Haml::Engine.new(template)
print haml.render(Object.new, :items => items)