#!/usr/bin/ruby

# https://secure.maestroweb.com/Login.aspx?OrgID=929
# https://secure.maestroweb.com/Details.aspx?OrgID=929&ItemID=1&selection=36

require 'rubygems'
require 'hpricot'
I_KNOW_I_AM_USING_AN_OLD_AND_BUGGY_VERSION_OF_LIBXML2 = true
require 'mechanize'
require 'pp'
require 'highline/import'
require 'builder'
#require 'uri'
#require 'net/http'
#require 'net/https'

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
  
  img_url = nil
  img_remote_url = 'https://secure.maestroweb.com/' + item.search('#imgItem').first['src']
  img_file = 'images/' + File.basename(img_remote_url)
  begin
    $agent.get(img_remote_url).save(img_file)
    img_url = 'file:///' + File.join(Dir.pwd, img_file)
  rescue
  end
  
  data = {
    :id => id,
    :name => item.search('#lblItemName').first.content.strip,
    :desc => item.search('#lblItemDesc').first.content.strip,
    :value => item.search('#lblItemValue').first.content.strip,
    :donor => item.search('#lblItemDonors').first.content.strip,
    :img => img_url
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

xml = Builder::XmlMarkup.new(:target=>STDOUT, :indent=>0)
xml.Root do |root|
  150.times do |i|
    item = get_item(i)
    $stderr.print(item ? '1' : '0')
    next unless item
    root.item do |b|
      b.image(:href => item[:img])
      puts
      b.name(item[:name])
      puts
      b.desc(item[:desc])
      puts
      b.value_label("Value: ")
      b.value(item[:value])
      puts
      b.donor_label("Donated by: ")
      b.donor(item[:donor])
      puts
      b.id("Auction lot #{item[:id]}")
      puts
    end
  end
end
